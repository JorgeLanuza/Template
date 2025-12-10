using System.Security.Claims;

using BaseCore.Framework.Security.Business.Helpers;
using BaseCore.Framework.Security.DataAccess.Entities.Authentication;
using BaseCore.Framework.Security.DataAccess.Entities.Authorization;
using BaseCore.Framework.Security.DataAccess.Repositories.Authentication;
using BaseCore.Framework.Security.DataAccess.Repositories.Authorization;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Template.WebApi.Controllers;

[ApiController]
[Route("connect")]
public class AuthorizationController(AuthenticationRepository authenticationRepository, UserRepository userRepository, RoleRepository roleRepository, UserApplicationRolesRepository userApplicationRolesRepository) : ControllerBase
{
	private readonly AuthenticationRepository _authenticationRepository = authenticationRepository;

	private readonly UserRepository _userRepository = userRepository;

	private readonly RoleRepository _roleRepository = roleRepository;

	private readonly UserApplicationRolesRepository _userApplicationRolesRepository = userApplicationRolesRepository;

	[HttpPost("token")]
	[Produces("application/json")]
	public async Task<IActionResult> Exchange()
	{
		OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
		if (request == null)
			return BadRequest("The OpenIddict server request cannot be retrieved.");

		if (request.IsPasswordGrantType())
		{
			string? authId = request.Username;
			AuthenticationEntity? auth = await _authenticationRepository.FindAsync(x => x!.AuthenticationId == authId);

			if (auth == null || !Cyphering.CheckPassword(request.Password!, auth.Password))
			{
				return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
					{
						[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant, [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid.",
					}));
			}

			UserEntity? user = await _userRepository.FindAsync(u => u!.AuthenticationId == authId);
			if (user == null || user.Disabled || user.Locked.GetValueOrDefault())
			{
				return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
				{
					[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
					[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is disabled or locked.",
				}));
			}

			ClaimsPrincipal principal = await CreatePrincipalAsync(user);

			return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
		}

		return BadRequest(new
		{
			Error = OpenIddictConstants.Errors.UnsupportedGrantType,
			ErrorDescription = "The specified grant type is not supported.",
		});
	}

	private async Task<ClaimsPrincipal> CreatePrincipalAsync(UserEntity user)
	{
		ClaimsIdentity identity = new(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

		identity.AddClaim(OpenIddictConstants.Claims.Subject, user.AuthenticationId ?? string.Empty);
		identity.AddClaim(OpenIddictConstants.Claims.Name, user.AuthenticationId ?? string.Empty);
		identity.AddClaim(OpenIddictConstants.Claims.Username, user.AuthenticationId ?? string.Empty);

		List<UserApplicationRolesEntity> userRoles = [.. _userApplicationRolesRepository.FindAll(x => x.UserId == user.Id)];
		foreach (UserApplicationRolesEntity userRole in userRoles)
		{
			RoleEntity role = await _roleRepository.GetAsync(userRole.RoleId);
			if (role != null)
				identity.AddClaim(OpenIddictConstants.Claims.Role, role.Name);
		}

		ClaimsPrincipal principal = new(identity);

		principal.SetScopes(new[]
		{
			// OpenIddictConstants.Scopes.OpenId, // Usually implicit
            "openid",
			OpenIddictConstants.Scopes.Email,
			OpenIddictConstants.Scopes.Profile,
			OpenIddictConstants.Scopes.Roles,
            OpenIddictConstants.Scopes.OfflineAccess, // Critical for RefreshToken
		});

		// if needed
		// principal.SetResources(...);

		return Task.FromResult(principal).Result;
	}
}
