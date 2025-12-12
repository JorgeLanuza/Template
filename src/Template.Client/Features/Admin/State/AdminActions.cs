using BaseCore.Framework.Security.Identity.Dtos;

using Template.Client.Features.Admin.Models;
// For AdminUserDto if local, or Framework if available? Framework doesn't likely have 'AdminUserDto' with custom logic. I'll use local model for Users grid.

namespace Template.Client.Features.Admin.State;

public record LoadDashboardAction();

public record DashboardLoadedAction(int TotalUsers, int ActiveSessions, int LockedUsers, int RecentErrors);

public record LoadPoliciesAction();

public record PoliciesLoadedAction(PasswordPolicyDto PasswordPolicy, LockingPolicyDto LockingPolicy);

public record LoadUsersAction();

public record UsersLoadedAction(List<AdminUserModel> Users);

public record AdminErrorAction(string ErrorMessage);

public record AdminLoadingAction(bool IsLoading);
