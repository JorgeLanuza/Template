using Microsoft.AspNetCore.Components;
using MudBlazor;
using Template.Client.Features.Auth.Models;
using Template.Client.Features.Auth.Services.Interfaces;

namespace Template.Client.Features.Auth.Pages;

public partial class ChangePassword
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private MudForm _form = null!;
    private bool _success;
    private bool _loading;
    private ChangePasswordModel _model = new();

    private string? PasswordMatch(string arg)
    {
        if (_model.NewPassword != arg)
            return "Passwords do not match.";
        return null;
    }

    private async Task SubmitAsync()
    {
        await _form.Validate();
        if (_form.IsValid)
        {
            _loading = true;
            bool result = await AuthService.ChangePasswordAsync(_model.OldPassword, _model.NewPassword);
            _loading = false;

            if (result)
            {
                Snackbar.Add("Password changed successfully!", Severity.Success);
                Navigation.NavigateTo("/profile");
            }
            else
            {
                Snackbar.Add("Failed to change password. Check your current password or policy requirements.", Severity.Error);
            }
        }
    }
}
