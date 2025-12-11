using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Template.Client.Features.Auth.Services.Interfaces;

namespace Template.Client.Features.Auth.Pages;

public partial class ResetPassword
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;

    // Note: In real flows, we would capture the 'Code' (Token) from Query String and pass it to API
    // [SupplyParameterFromQuery] private string? Code { get; set; }

    private bool success;
    private MudForm? form;
    private ResetInputModel Input = new();

    private class ResetInputModel
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? ConfirmPassword { get; set; }
    }

    private async Task OnResetPasswordAsync()
    {
        await form!.Validate();
        if (!form.IsValid) return;

        if (Input.Password != Input.ConfirmPassword)
        {
            Snackbar.Add("Passwords do not match.", Severity.Error);
            return;
        }

        bool result = await AuthService.ResetPasswordAsync(Input.Username!, Input.Password!);
        
        if (result)
        {
            Snackbar.Add("Password reset successfully. You can login now.", Severity.Success);
            Navigation.NavigateTo("/login");
        }
        else
        {
             Snackbar.Add("Failed to reset password. Ensure policies are met.", Severity.Error);
        }
    }
}
