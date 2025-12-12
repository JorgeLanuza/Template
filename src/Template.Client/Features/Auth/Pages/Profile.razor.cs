using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Template.Client.Features.Auth.Pages;

public partial class Profile
{
	[Inject]
	private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;

	private ClaimsPrincipal? _user;

	protected override async Task OnInitializedAsync()
	{
		var authState = await AuthStateProvider.GetAuthenticationStateAsync();
		_user = authState.User;
	}

	private string GetClaimValue(string claimType) => _user?.Claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? "N/A";

	// Fallback helpers in case mapping varies
	private string GetFullName()
	{
		var first = GetClaimValue("given_name");
		var last = GetClaimValue("family_name");
		var name = _user?.Identity?.Name;

		if (first != "N/A" || last != "N/A")
		{
			return $"{first} {last}".Trim();
		}

		return name ?? "Unknown User";
	}

	private string GetEmail() => _user?.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == ClaimTypes.Email)?.Value ?? "No Email";

	private string GetUserId() => _user?.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value ?? "N/A";

	private string GetInitials()
	{
		var first = GetClaimValue("given_name");
		if (first != "N/A" && first.Length > 0)
			return first.Substring(0, 1).ToUpper();

		return _user?.Identity?.Name?[..1].ToUpper() ?? "?";
	}

	// IsActive isn't standard in token usually, but if we assume it might be there:
	// If not, we just assume active because they are logged in. 
	// Usually IdentityServer does not issue tokens to inactive users.
	private bool IsActive() => true;
}
