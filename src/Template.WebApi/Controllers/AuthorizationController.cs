using System.Security.Claims;

using BaseCore.Framework.Security.Business.Services;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Template.WebApi.Controllers;

public class AuthorizationController(IdentityAuthenticationService authenticationService) : ControllerBase
{
	private readonly IdentityAuthenticationService _authenticationService = authenticationService;

	[HttpPost("token")]
	[Produces("application/json")]
	public async Task<IActionResult> Exchange()
	{
		OpenIddictRequest? request = HttpContext.GetOpenIddictServerRequest();
		if (request is null)
			return BadRequest("The OpenIddict server request cannot be retrieved.");

		if (request.IsPasswordGrantType())
		{
			ClaimsPrincipal? principal = await _authenticationService.AuthenticateAsync(request.Username ?? string.Empty, request.Password ?? string.Empty);

			if (principal is null)
			{
				return Forbid(authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, properties: new AuthenticationProperties(new Dictionary<string, string?>
				{
					[OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
					[OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid or the user is disabled.",
				}));
			}

			// Set the scopes granted to the client
			principal.SetScopes(request.GetScopes());

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
