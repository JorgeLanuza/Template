using BaseCore.Framework.ExceptionManager;
using BaseCore.Framework.Observability.Logging.Interfaces;
using BaseCore.Framework.Security.Identity.Dtos;

namespace Template.Client.Features.Admin.Services;

public class AdminService(HttpClient httpClient) : IAdminService
{
	private readonly HttpClient _httpClient = httpClient;

	public async Task<PasswordPolicyDto?> GetPasswordPolicyAsync()
	{
		try
		{
			PasswordPolicyDto? passwordPolicy = await _httpClient.GetFromJsonAsync<PasswordPolicyDto>("api/admin/policies/password");
			return passwordPolicy;
		}
		catch (HttpRequestException httpRequestException)
		{
			Console.WriteLine($"Network error getting password policy: {httpRequestException.Message}");
			return null;
		}
		catch (Exception exception)
		{
			Console.WriteLine($"Unexpected error getting password policy: {exception.Message}");

			return null;
		}
	}

	public async Task<bool> UpdatePasswordPolicyAsync(PasswordPolicyDto policy)
	{
		try
		{
			HttpResponseMessage responseMessage = await _httpClient.PutAsJsonAsync("api/admin/policies/password", policy);
			return responseMessage.IsSuccessStatusCode;
		}
		catch (Exception exception)
		{
			Console.WriteLine($"Error updating password policy: {exception.Message}");

			return false;
		}
	}

	public async Task<LockingPolicyDto?> GetLockingPolicyAsync()
	{
		try
		{
			LockingPolicyDto? lockingPolicy = await _httpClient.GetFromJsonAsync<LockingPolicyDto>("api/admin/policies/locking");

			return lockingPolicy;
		}
		catch (HttpRequestException httpRequestException)
		{
			Console.WriteLine($"Network error getting locking policy: {httpRequestException.Message}");

			return null;
		}
		catch (Exception exception)
		{
			Console.WriteLine($"Unexpected error getting locking policy: {exception.Message}");

			return null;
		}
	}

	public async Task<bool> UpdateLockingPolicyAsync(LockingPolicyDto policy)
	{
		try
		{
			HttpResponseMessage responseMessage = await _httpClient.PutAsJsonAsync("api/admin/policies/locking", policy);
			return responseMessage.IsSuccessStatusCode;
		}
		catch (Exception exception)
		{
			Console.WriteLine($"Error updating locking policy: {exception.Message}");
			return false;
		}
	}
}
