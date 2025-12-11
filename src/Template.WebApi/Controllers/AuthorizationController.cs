using System.Security.Claims;
using BaseCore.Framework.Security.DataAccess.Entities.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Template.WebApi.Controllers;

public class AuthorizationController(
	BaseCore.Framework.Security.Business.Services.AuthenticationService authenticationService,
	IPasswordHasher<AuthenticationEntity> passwordHasher,
	BaseCore.Framework.Security.DataAccess.Repositories.Authentication.AuthenticationRepository authenticationRepository,
	BaseCore.Framework.Security.DataAccess.Repositories.Authentication.UserRepository userRepository) : ControllerBase
{
	private readonly BaseCore.Framework.Security.Business.Services.AuthenticationService _authenticationService = authenticationService;
	private readonly IPasswordHasher<AuthenticationEntity> _passwordHasher = passwordHasher;
	private readonly BaseCore.Framework.Security.DataAccess.Repositories.Authentication.AuthenticationRepository _authenticationRepository = authenticationRepository;
	private readonly BaseCore.Framework.Security.DataAccess.Repositories.Authentication.UserRepository _userRepository = userRepository;

	[HttpGet("debug-hash")]
	public IActionResult GetHash([FromQuery] string password)
	{
		AuthenticationEntity dummyUser = new() { AuthenticationId = "debug" };
		return Ok(_passwordHasher.HashPassword(dummyUser, password));
	}

	[HttpPost("token")]
	[Produces("application/json")]
	public async Task<IActionResult> Exchange()
	{
		OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
		if (request == null)
			return BadRequest("The OpenIddict server request cannot be retrieved.");

		if (request.IsPasswordGrantType())
		{
			ClaimsPrincipal? principal = await _authenticationService.AuthenticateAndCreatePrincipalAsync(request.Username ?? string.Empty, request.Password ?? string.Empty);

			if (principal is null)
			{
				return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
				{
					[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
					[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid or the user is disabled.",
				}));
			}

			// OpenIddict: Set the scopes granted to the client
			principal.SetScopes(request.GetScopes());

			// OpenIddict: Set the destinations of the claims (AccessToken vs IdentityToken)
			foreach (Claim claim in principal.Claims)
			{
				claim.SetDestinations(GetDestinations(claim));
			}

			return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
		}

		return BadRequest(new
		{
			Error = OpenIddictConstants.Errors.UnsupportedGrantType,
			ErrorDescription = "The specified grant type is not supported.",
		});
	}

	private static IEnumerable<string> GetDestinations(Claim claim)
	{

		switch (claim.Type)
		{
			case OpenIddictConstants.Claims.Name:
			case OpenIddictConstants.Claims.Email:
			case OpenIddictConstants.Claims.Subject:
			case OpenIddictConstants.Claims.Role:
				yield return OpenIddictConstants.Destinations.AccessToken;
				yield return OpenIddictConstants.Destinations.IdentityToken;
				break;

			case "AspNet.Identity.SecurityStamp": 
				yield break;

			default:
				yield return OpenIddictConstants.Destinations.AccessToken;
				break;
		}
	}
}
