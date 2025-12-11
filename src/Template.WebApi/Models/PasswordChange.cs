using System.ComponentModel.DataAnnotations;

namespace Template.WebApi.Models;

public class PasswordChange
{
	[Required]
	public string OldPassword { get; set; } = string.Empty;

	[Required]
	public string NewPassword { get; set; } = string.Empty;

	[Compare("NewPassword")]
	public string ConfirmPassword { get; set; } = string.Empty;
}
