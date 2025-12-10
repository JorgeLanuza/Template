using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

using Template.Client.Features.Auth.Services.Interfaces;
using Template.Client.Features.Auth.Models;

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

			// Notify AuthStateProvider
			if (_authStateProvider is CustomAuthStateProvider customProvider)
			{
				customProvider.NotifyUserAuthentication(tokenResult.AccessToken);
			}

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public async Task LogoutAsync()
	{
		await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
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

}
