using System.Security.Claims;
using System.Text.Json;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Template.Client.Features.Auth.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
	private readonly IJSRuntime _jsRuntime;
	private readonly AuthenticationState _anonymous;

	public CustomAuthStateProvider(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
		_anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
	}

	public override async Task<AuthenticationState> GetAuthenticationStateAsync()
	{
		try
		{
			string? token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "idToken");

			if (string.IsNullOrWhiteSpace(token))
				return _anonymous;

			return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt")));
		}
		catch (Exception)
		{
			// Likely prerendering or JS not available
			return _anonymous;
		}
	}

	public void NotifyUserAuthentication(string token)
	{
		IEnumerable<Claim>? claims = ParseClaimsFromJwt(token);
		ClaimsPrincipal authenticatedUser = new(new ClaimsIdentity(claims, "jwt"));
		Task<AuthenticationState>? authState = Task.FromResult(new AuthenticationState(authenticatedUser));
		NotifyAuthenticationStateChanged(authState);
	}

	public void NotifyUserLogout()
	{
		Task<AuthenticationState>? authState = Task.FromResult(_anonymous);
		NotifyAuthenticationStateChanged(authState);
	}

	private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
	{
		List<Claim> claims = [];
		string payload = string.Empty;

		string jsonString = string.Empty;

		try
		{
			string[]? parts = jwt.Split('.');
			if (parts.Length < 2)
			{
				return claims;
			}

			payload = parts[1];
			var jsonBytes = ParseBase64WithoutPadding(payload);

			using var doc = JsonDocument.Parse(jsonBytes);
			foreach (var element in doc.RootElement.EnumerateObject())
			{
				switch (element.Value.ValueKind)
				{
					case JsonValueKind.Array:
						foreach (var item in element.Value.EnumerateArray())
						{
							claims.Add(new Claim(element.Name, item.ToString()));
						}

						break;
					default:
						claims.Add(new Claim(element.Name, element.Value.ToString()));
						break;
				}
			}
		}
		catch (Exception)
		{
			// Invalid token format
		}

		return claims;
	}

	private static byte[] ParseBase64WithoutPadding(string base64)
	{
		base64 = base64.Replace('-', '+').Replace('_', '/');
		switch (base64.Length % 4)
		{
			case 2:
				base64 += "==";
				break;
			case 3:
				base64 += "=";
				break;
		}

		return Convert.FromBase64String(base64);
	}
}
