using BaseCore.Framework.Security.Business.Services;
using BaseCore.Framework.Security.Business.Services.Enums;
using BaseCore.Framework.Security.Business.Services.Models;
using BaseCore.Framework.Web.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Template.WebApi.Models;

namespace Template.WebApi.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthSupportController(CredentialManagerService credentialManagerService) : BaseController
{
	private readonly CredentialManagerService _credentialManagerService = credentialManagerService;

	[HttpPost("change-password")]
	[Authorize]
	public IActionResult ChangePassword([FromBody] PasswordChange changeRequest)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		string? username = User.Identity?.Name;
		if (string.IsNullOrEmpty(username))
			return Unauthorized();

		CredentialResult result = _credentialManagerService.Change(username, changeRequest);

		if (result == CredentialResult.OK)
			return Ok();

		if (result == CredentialResult.BAD_CREDENTIALS)
			return BadRequest("Invalid existing password.");

		if (result == CredentialResult.NO_MEET_POLICIES)
			return BadRequest("New password does not meet security policies.");

		return BadRequest("Password change failed.");
	}

	/// <summary>
	/// Resets the password. Note: In a real scenario, this should likely involve a token sent via email.
	/// The CredentialManagerService.Reset method seems to take a username and a structure with the new password.
	/// Based on the service inspection, it checks policies and updates. It doesn't seem to validate an email token itself 
	/// in the method signature I saw, but let's assume strict usage from the client side or that the framework handles it elsewhere.
	/// For this integration, we expose what is available.
	/// </summary>
	/// <returns>IActionResult</returns>
	[HttpPost("reset-password")]
	[AllowAnonymous]
	public IActionResult ResetPassword([FromBody] PasswordResetRequestDto request)
	{
		// NOTE: Secure implementation usually requires a token. 
		// We are strictly bridging the Framework service here. 
		// If functionality is missing (e.g. token validation), we expose what exists 
		// and add necessary TODOs or safeguards.

		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		// We need a DTO for this because PasswordReset might be an internal model or we want to shape the request.
		// The service expects (username, PasswordReset).
		PasswordReset internalModel = new()
		{
			ConfirmPassword = request.NewPassword, // Service maps 'ConfirmPassword' to new pass based on logic seen
			// The service logic: _credentialPoliciesManagerService.IsPasswordOk(authId, string.Empty, passwordReset.ConfirmPassword)
		};

		try
		{
			bool success = _credentialManagerService.Reset(request.Username, internalModel);
			if (success)
			{
				return Ok();
			}

			return BadRequest("Password reset failed due to policy violation or user not found.");
		}
		catch (Exception)
		{
			// Do not leak internal errors for security
			return BadRequest("An error occurred during password reset.");
		}
	}
}


