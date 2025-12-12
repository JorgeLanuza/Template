using Microsoft.AspNetCore.Components;

using MudBlazor;

using Template.Client.Features.Auth.Services.Interfaces;

namespace Template.Client.Features.Auth.Pages;

public partial class ForgotPassword
{
	[Inject]
	private IAuthService AuthService { get; set; } = null!;

	[Inject]
	private ISnackbar Snackbar { get; set; } = null!;

	[Inject]
	private NavigationManager Navigation { get; set; } = null!;

	private string? email;

	private MudForm? form;

	private bool _loading;

	private async Task OnForgotPasswordAsync()
	{
		await form!.Validate();
		if (!form.IsValid)
			return;

		_loading = true;
		await AuthService.ForgotPasswordAsync(email!);
		_loading = false;

		// Always show success to prevent enumeration
		Snackbar.Add("If the email exists, a reset link has been sent.", Severity.Success);
	}
}
