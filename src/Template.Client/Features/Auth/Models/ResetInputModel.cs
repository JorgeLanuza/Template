namespace Template.Client.Features.Auth.Models;

public class ResetInputModel
{
	public string? Username { get; set; }

	public string? Password { get; set; }

	public string? ConfirmPassword { get; set; }
}
