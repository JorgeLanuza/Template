using Microsoft.AspNetCore.Components;

using MudBlazor;

using Template.Client.Features.Admin.Components;
using Template.Client.Features.Admin.Models;
using Template.Client.Features.Admin.State;
using Template.Client.State;

namespace Template.Client.Features.Admin.Pages;

public partial class Users
{
	[Inject]
	private ISnackbar Snackbar { get; set; } = default!;

	[Inject]
	private IDialogService DialogService { get; set; } = default!;

	[Inject]
	private Store Store { get; set; } = default!;

	private AdminState _state = new();

	private string searchString = string.Empty;

	protected override async Task OnInitializedAsync()
	{
		_state = Store.GetState<AdminState>() ?? new AdminState();
		Store.Subscribe<AdminState>(UpdateState);

		await LoadUsers();
	}

	// Quick filter
	private static bool FilterFunc(AdminUserModel user, string searchString)
	{
		if (string.IsNullOrWhiteSpace(searchString))
			return true;

		if (user.UserName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
			return true;

		if (user.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
			return true;
		return false;
	}

	private bool FilterFunc1(AdminUserModel user) => FilterFunc(user, searchString);

	private void UpdateState(AdminState newState)
	{
		_state = newState;
		InvokeAsync(StateHasChanged);
	}

	private async Task LoadUsers()
	{
		Store.Dispatch(new LoadUsersAction());

		// Mock Data for UI Verification if Backend fails
		await Task.Delay(500);
		var mockUsers = new List<AdminUserModel>
		{
			 new() { Id = "1", UserName = "admin", Email = "admin@basecore.com", Roles = new() { "SuperAdmin" }, LastActive = DateTime.Now, LastIp = "127.0.0.1" },
			 new() { Id = "2", UserName = "jdoe", Email = "john.doe@example.com", Roles = new() { "User" }, LastActive = DateTime.Now.AddMinutes(-15), LastIp = "192.168.1.5" },
			 new() { Id = "3", UserName = "lockeduser", Email = "bad@actor.com", Roles = new() { "User" }, IsLocked = true, LastActive = DateTime.Now.AddDays(-1), LastIp = "10.0.0.99" },
			 new() { Id = "4", UserName = "alice", Email = "alice@example.com", Roles = new() { "Manager" }, LastActive = DateTime.Now.AddHours(-2), LastIp = "10.0.0.50" },
		};

		Store.Dispatch(new UsersLoadedAction(mockUsers));
	}

	private async Task OpenUserDialog(AdminUserModel? user = null)
	{
		DialogParameters parameters = [];
		if (user is not null)
			parameters.Add("User", user);

		DialogOptions options = new() { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true };
		IDialogReference dialog = await DialogService.ShowAsync<AdminUserDialog>(user == null ? "Create User" : "Edit User", parameters, options);
		DialogResult? result = await dialog.Result;

		if (!result!.Canceled)
		{
			// Reload users after mock save
			Snackbar.Add("User saved successfully", Severity.Success);
			await LoadUsers();
		}
	}

	private async Task UnlockUser(AdminUserModel user)
	{
		// Call Service...
		Snackbar.Add($"User {user.UserName} unlocked", Severity.Success);

		// Mock State Update
		AdminUserModel? userInList = _state.Users.FirstOrDefault(u => u.Id == user.Id);
		if (userInList is not null)
			userInList.IsLocked = false;

		Store.Dispatch(new UsersLoadedAction(_state.Users)); // Refresh Grid
	}

	private async Task ResetPassword(AdminUserModel user)
	{
		bool? result = await DialogService.ShowMessageBox("Warning", $"Reset password for {user.UserName}?", yesText: "Reset", cancelText: "Cancel");

		if (result == true)
		{
			Snackbar.Add($"Password reset email sent to {user.Email}", Severity.Success);
		}
	}

	private async Task RevokeSession(AdminUserModel user)
	{
		bool? result = await DialogService.ShowMessageBox("Panic Button", $"Revoke ALL sessions for {user.UserName}? They will be logged out immediately.", yesText: "Revoke!", cancelText: "Cancel");

		if (result == true)
		{
			Snackbar.Add($"Sessions revoked for {user.UserName}", Severity.Warning);
		}
	}
}
