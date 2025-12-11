namespace Template.Client.Features.Auth.Services.Interfaces;

public interface IAuthService
{
	Task<bool> LoginAsync(string username, string password);

	Task LogoutAsync();

	Task<bool> ChangePasswordAsync(string oldPassword, string newPassword);

	Task<bool> ResetPasswordAsync(string username, string newPassword);

	Task<bool> ForgotPasswordAsync(string email);
}
