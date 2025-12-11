namespace Template.Client.Features.Auth.Components;

public partial class PasswordGenerator
{
	private string generatedPassword = string.Empty;

	private int length = 12;

	private bool useSpecial = true;

	private bool useNumbers = true;

	private string specialLabel = "Special Characters (!@#$)";

	private void Generate()
	{
		const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

		const string special = "!@#$%^&*";
		const string numbers = "1234567890";

		string? chars = valid;
		if (useSpecial)
			chars += special;
		if (useNumbers)
			chars += numbers;

		Random rnd = new();
		generatedPassword = new string([.. Enumerable.Repeat(chars, length).Select(s => s[rnd.Next(s.Length)])]);
	}

	private void CopyPassword()
	{
		// Copy logic (requires JSInterop, skipping for simplicity)
	}
}
