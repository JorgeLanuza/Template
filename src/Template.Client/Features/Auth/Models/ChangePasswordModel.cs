using System.ComponentModel.DataAnnotations;

namespace Template.Client.Features.Auth.Models;

public class ChangePasswordModel
{
    [Required]
    public string OldPassword { get; set; } = string.Empty;

    [Required]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
