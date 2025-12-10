namespace Template.Client.Features.Auth.State;

public record AuthState
{
	public string GeneratedPassword { get; init; } = string.Empty;

	public bool IsLoading { get; init; } = false;

	public string? ErrorMessage { get; init; } = null;

	public PasswordConfiguration Configuration { get; init; } = new();

	// Login State
	public bool IsAuthenticated { get; init; } = false;

	public string? Token { get; init; } = null;
}

#pragma warning disable SA1402
public record PasswordConfiguration
#pragma warning restore SA1402
{
	public int Length { get; init; } = 12;

	public bool UseNumbers { get; init; } = true;

	public bool UseSpecial { get; init; } = true;
}
