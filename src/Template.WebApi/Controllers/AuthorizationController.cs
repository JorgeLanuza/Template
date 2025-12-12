using System.Security.Claims;

using BaseCore.Framework.Security.Identity.Entities;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Template.WebApi.Controllers;

[Route("api/[controller]")]
public class AuthorizationController(UserManager<BaseUser> userManager, SignInManager<BaseUser> signInManager) : ControllerBase
{
	private readonly UserManager<BaseUser> _userManager = userManager;

	private readonly SignInManager<BaseUser> _signInManager = signInManager;

	[HttpPost("token")]
	[Produces("application/json")]
	public async Task<IActionResult> Exchange()
	{
		OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
		if (request is null)
			return BadRequest("The OpenIddict server request cannot be retrieved.");

		if (!request.IsPasswordGrantType())
		{
			return BadRequest(new
			{
				Error = OpenIddictConstants.Errors.UnsupportedGrantType,
				ErrorDescription = "The specified grant type is not supported.",
			});
		}

		Console.WriteLine($"[AuthDebug] Attempting login for user: '{request.Username}'");

		BaseUser? user = await _userManager.FindByNameAsync(request.Username ?? string.Empty);
		Console.WriteLine($"[AuthDebug] User found: {user != null}");

		if (user is null)
		{
			return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
			{
				[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
				[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid or the user is disabled.",
			}));
		}

		bool isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password ?? string.Empty);
		Console.WriteLine($"[AuthDebug] Password valid: {isPasswordValid}");

		if (!isPasswordValid)
		{
			return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
			{
				[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
				[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid or the user is disabled.",
			}));
		}

		// Ensure user is allowed to sign in (not locked out, etc)
		if (!await _signInManager.CanSignInAsync(user))
		{
			Console.WriteLine("[AuthDebug] User NOT allowed to sign in (Locked out or not allowed).");
			return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
			{
				[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
				[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid or the user is disabled.",
			}));
		}

		Console.WriteLine("[AuthDebug] User allowed to sign in. Creating principal...");
		// Create the principal
		ClaimsPrincipal principal = await _signInManager.CreateUserPrincipalAsync(user);
		Console.WriteLine($"[AuthDebug] Principal created: {principal != null}");

		// Set the scopes granted to the client
		principal!.SetScopes(request.GetScopes());

		foreach (Claim claim in principal!.Claims)
		{
			claim.SetDestinations(GetDestinations(claim));
		}

		return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
	}

	private static IEnumerable<string> GetDestinations(Claim claim)
	{
		switch (claim.Type)
		{
			case OpenIddictConstants.Claims.Name:
			case OpenIddictConstants.Claims.Email:
			case OpenIddictConstants.Claims.Subject:
			case OpenIddictConstants.Claims.Role:
			case OpenIddictConstants.Claims.PreferredUsername:
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
