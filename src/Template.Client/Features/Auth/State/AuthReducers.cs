using Template.Client.State;

namespace Template.Client.Features.Auth.State;

public static class AuthReducers
{
	public static void Initialize(Store store)
	{
		store.Subscribe<GeneratePasswordAction>(action => GeneratePasswordReducer(store, action));
		store.Subscribe<LoginAction>(action => LoginReducer(store, action));
		store.Subscribe<LoginSuccessAction>(action => LoginSuccessReducer(store, action));
		store.Subscribe<LoginFailureAction>(action => LoginFailureReducer(store, action));
	}

	private static void GeneratePasswordReducer(Store store, GeneratePasswordAction action)
	{
		// Map UI flags to Complexity level:
		// 1 Lower
		// 2 Lower + Upper (Default base)
		// 3 + Numbers
		// 4 + Special

		int complexity = 2; // Start with Lower + Upper
		if (action.UseNumbers)
			complexity++;

		if (action.UseSpecial)
			complexity++;

		// Cap at 4
		if (complexity > 4)
			complexity = 4;

		try
		{
			string generatedPassword = GeneratePassword(action.Length, complexity);

			// Update State
			AuthState? currentState = store.GetState<AuthState>();
			AuthState? newState = currentState with
			{
				GeneratedPassword = generatedPassword,
				Configuration = new PasswordConfiguration
				{
					Length = action.Length,
					UseNumbers = action.UseNumbers,
					UseSpecial = action.UseSpecial,
				},
			};

			store.SetState(newState);
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error generating password: {ex.Message}");
		}
	}

	private static void LoginReducer(Store store, LoginAction action)
	{
		AuthState? currentState = store.GetState<AuthState>();

		store.SetState(currentState with { IsLoading = true, ErrorMessage = null });
	}

	private static void LoginSuccessReducer(Store store, LoginSuccessAction action)
	{
		AuthState? currentState = store.GetState<AuthState>();

		store.SetState(currentState with { IsLoading = false, IsAuthenticated = true, Token = action.Token, ErrorMessage = null });
	}

	private static void LoginFailureReducer(Store store, LoginFailureAction action)
	{
		AuthState? currentState = store.GetState<AuthState>();

		store.SetState(currentState with { IsLoading = false, IsAuthenticated = false, Token = null, ErrorMessage = action.Error });
	}

	private static string GeneratePassword(int length, int complexity)
	{
		const string lower = "abcdefghijklmnopqrstuvwxyz";
		const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		const string digits = "0123456789";
		const string special = "!@#$%^&*()_+";

		string chars = lower;
		if (complexity >= 2)
			chars += upper;

		if (complexity >= 3)
			chars += digits;

		if (complexity >= 4)
			chars += special;

		Random random = new();

		return new string([.. Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)])]);
	}
}
