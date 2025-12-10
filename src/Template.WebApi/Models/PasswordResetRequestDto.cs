namespace Template.WebApi.Models;

public class PasswordResetRequestDto
{
	public string Username { get; set; } = string.Empty;

	public string NewPassword { get; set; } = string.Empty;
}
