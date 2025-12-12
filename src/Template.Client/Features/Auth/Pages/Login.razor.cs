using Microsoft.AspNetCore.Components;

using MudBlazor;

using Template.Client.Features.Auth.Models;
using Template.Client.Features.Auth.Services.Interfaces;
using Template.Client.Features.Auth.State;
using Template.Client.State;

namespace Template.Client.Features.Auth.Pages;

public partial class Login
{
	[Inject]
	private IAuthService AuthService { get; set; } = null!;

	[Inject]
	private ISnackbar Snackbar { get; set; } = null!;

	[Inject]
	private NavigationManager Navigation { get; set; } = null!;

	[Inject]
	private Store Store { get; set; } = null!;

	private MudForm _form = default!;

	private bool _success;

	private AuthState _state = new();

	private LoginViewModel _model = new();

	protected override void OnInitialized()
	{
		_state = Store.GetState<AuthState>();
		Store.Subscribe<AuthState>(UpdateState);
	}

	private void UpdateState(AuthState newState)
	{
		_state = newState;
		InvokeAsync(StateHasChanged);
	}

	private async Task SubmitAsync()
	{
		Console.WriteLine("SubmitAsync Clicked");
		await _form.Validate();

		if (_form.IsValid)
		{
			Console.WriteLine("Form Is Valid. Attempting Login...");
			Store.Dispatch(new LoginAction());

			bool result = await AuthService.LoginAsync(_model.Username, _model.Password);

			if (result)
			{
				Store.Dispatch(new LoginSuccessAction("token-placeholder-managed-by-service"));
				Navigation.NavigateTo("/");
			}
			else
			{
				Store.Dispatch(new LoginFailureAction("Invalid username or password."));
				Snackbar.Add("Invalid username or password.", Severity.Error);
			}
		}
		else
		{
			Snackbar.Add("Please fill in all required fields.", Severity.Warning);
		}
	}
}
