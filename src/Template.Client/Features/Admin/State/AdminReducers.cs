using Template.Client.State;

namespace Template.Client.Features.Admin.State;

public static class AdminReducers
{
	public static void Initialize(Store store)
	{
		store.Subscribe<LoadDashboardAction>(action => LoadDashboardReducer(store));
		store.Subscribe<DashboardLoadedAction>(action => DashboardLoadedReducer(store, action));
		store.Subscribe<LoadPoliciesAction>(action => LoadPoliciesReducer(store));
		store.Subscribe<PoliciesLoadedAction>(action => PoliciesLoadedReducer(store, action));
		store.Subscribe<LoadUsersAction>(action => LoadUsersReducer(store));
		store.Subscribe<UsersLoadedAction>(action => UsersLoadedReducer(store, action));
		store.Subscribe<AdminErrorAction>(action => ErrorReducer(store, action));
	}

	private static void LoadDashboardReducer(Store store)
	{
		var state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with { IsLoading = true, ErrorMessage = null });
	}

	private static void DashboardLoadedReducer(Store store, DashboardLoadedAction action)
	{
		var state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with
		{
			IsLoading = false,
			TotalUsers = action.TotalUsers,
			ActiveSessions = action.ActiveSessions,
			LockedUsers = action.LockedUsers,
			RecentErrors = action.RecentErrors,
		});
	}

	private static void LoadPoliciesReducer(Store store)
	{
		AdminState? state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with { IsLoading = true, ErrorMessage = null });
	}

	private static void PoliciesLoadedReducer(Store store, PoliciesLoadedAction action)
	{
		var state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with
		{
			IsLoading = false,
			PasswordPolicy = action.PasswordPolicy,
			LockingPolicy = action.LockingPolicy,
		});
	}

	private static void LoadUsersReducer(Store store)
	{
		AdminState? state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with { IsLoading = true, ErrorMessage = null });
	}

	private static void UsersLoadedReducer(Store store, UsersLoadedAction action)
	{
		var state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with { IsLoading = false, Users = action.Users });
	}

	private static void ErrorReducer(Store store, AdminErrorAction action)
	{
		var state = store.GetState<AdminState>() ?? new AdminState();
		store.SetState(state with { IsLoading = false, ErrorMessage = action.ErrorMessage });
	}
}
