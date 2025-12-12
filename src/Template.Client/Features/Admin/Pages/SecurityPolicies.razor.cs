using Microsoft.AspNetCore.Components;

using MudBlazor;

using BaseCore.Framework.Security.Identity.Dtos;

using Template.Client.Features.Admin.Services;
using Template.Client.Features.Admin.State;
using Template.Client.State;

namespace Template.Client.Features.Admin.Pages;

public partial class SecurityPolicies
{
	[Inject]
	private ISnackbar Snackbar { get; set; } = default!;

	[Inject]
	private IAdminService AdminService { get; set; } = default!;

	[Inject]
	private Store Store { get; set; } = default!;

	private AdminState _state = new();

	private MudForm? passwordForm;

	private bool passwordSuccess;

	private MudForm? lockingForm;

	private bool lockingSuccess;

	private PasswordPolicyDto PasswordPolicy => _state.PasswordPolicy ?? new PasswordPolicyDto();

	private LockingPolicyDto LockingPolicy => _state.LockingPolicy ?? new LockingPolicyDto();

	protected override async Task OnInitializedAsync()
	{
		_state = Store.GetState<AdminState>() ?? new AdminState();
		Store.Subscribe<AdminState>(UpdateState);

		await LoadData();
	}

	private void UpdateState(AdminState newState)
	{
		_state = newState;
		InvokeAsync(StateHasChanged);
	}

	private async Task LoadData()
	{
		Store.Dispatch(new LoadPoliciesAction());

		try
		{
			PasswordPolicyDto pwd = await AdminService.GetPasswordPolicyAsync() ?? new PasswordPolicyDto();
			LockingPolicyDto loc = await AdminService.GetLockingPolicyAsync() ?? new LockingPolicyDto();

			Store.Dispatch(new PoliciesLoadedAction(pwd, loc));
		}
		catch (Exception ex)
		{
			Store.Dispatch(new AdminErrorAction(ex.Message));
			Snackbar.Add("Failed to load policies", Severity.Error);
		}
	}

	private async Task SavePasswordPolicy()
	{
		await passwordForm!.Validate();
		if (!passwordForm.IsValid)
			return;

		bool success = await AdminService.UpdatePasswordPolicyAsync(PasswordPolicy);
		if (success)
		{
			Snackbar.Add("Password policy updated successfully.", Severity.Success);
			// Ideally re-fetch or dispatch updated state if backend modifies it
		}
		else
		{
			Snackbar.Add("Failed to update password policy.", Severity.Error);
		}
	}

	private async Task SaveLockingPolicy()
	{
		await lockingForm!.Validate();

		if (!lockingForm.IsValid)
			return;

		bool success = await AdminService.UpdateLockingPolicyAsync(LockingPolicy);
		if (success)
		{
			Snackbar.Add("Locking policy updated successfully.", Severity.Success);
		}
		else
		{
			Snackbar.Add("Failed to update locking policy.", Severity.Error);
		}
	}
}
