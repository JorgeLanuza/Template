using BaseCore.Framework.Security.Identity.Dtos;

using Template.Client.Features.Admin.Models;

namespace Template.Client.Features.Admin.State;

public record AdminState
{
	public bool IsLoading { get; init; }

	public string? ErrorMessage { get; init; }

	// Policies
	public PasswordPolicyDto? PasswordPolicy { get; init; }

	public LockingPolicyDto? LockingPolicy { get; init; }

	// Users
	public List<AdminUserModel> Users { get; init; } = [];

	// Dashboard
	public int TotalUsers { get; init; }

	public int ActiveSessions { get; init; }

	public int LockedUsers { get; init; }

	public int RecentErrors { get; init; }
}
