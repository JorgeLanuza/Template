using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using Template.Client.Features.Auth.Models;
using Template.Client.Features.Auth.Services.Interfaces;

namespace Template.Client.Features.Auth.Services;

public class AuthService(HttpClient httpClient, IJSRuntime jsRuntime, AuthenticationStateProvider authStateProvider, IConfiguration configuration) : IAuthService
{
	private readonly HttpClient _httpClient = httpClient;

	private readonly IJSRuntime _jsRuntime = jsRuntime;

	private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

	private readonly IConfiguration _configuration = configuration;

	public async Task<bool> LoginAsync(string username, string password)
	{
		try
		{
			string? clientId = _configuration["IdentityServerSettings:ClientSettings:ClientId"] ?? "BaseCore";

			Dictionary<string, string> keyValues = new()
			{
				{ "grant_type", "password" },
				{ "username", username },
				{ "password", password },
				{ "scope", "openid profile email offline_access" },
				{ "client_id", clientId },
			};

			FormUrlEncodedContent content = new(keyValues);

			HttpResponseMessage response = await _httpClient.PostAsync("connect/token", content);

			if (!response.IsSuccessStatusCode)
				return false;

			TokenResponse? tokenResult = await response.Content.ReadFromJsonAsync<TokenResponse>();

			if (tokenResult == null || string.IsNullOrEmpty(tokenResult.AccessToken))
				return false;

			await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", tokenResult.AccessToken);
			await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "idToken", tokenResult.IdToken);

			// Notify AuthStateProvider using the ID Token (which contains the user profile)
			if (_authStateProvider is CustomAuthStateProvider customProvider)
				customProvider.NotifyUserAuthentication(tokenResult.IdToken);

			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"[AuthService] LoginAsync Error: {ex.Message}");
			return false;
		}
	}

	public async Task LogoutAsync()
	{
		await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
		await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "idToken");
		if (_authStateProvider is CustomAuthStateProvider customProvider)
		{
			customProvider.NotifyUserLogout();
		}
	}

	public async Task<bool> ChangePasswordAsync(string oldPassword, string newPassword)
	{
		var request = new { OldPassword = oldPassword, NewPassword = newPassword };
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/change-password", request);

		return response.IsSuccessStatusCode;
	}

	public async Task<bool> ResetPasswordAsync(string username, string newPassword)
	{
		var request = new { Username = username, NewPassword = newPassword };
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", request);

		return response.IsSuccessStatusCode;
	}

	public async Task<bool> ForgotPasswordAsync(string email)
	{
		var request = new { Email = email };
		HttpResponseMessage response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", request);

		return response.IsSuccessStatusCode;
	}
}
