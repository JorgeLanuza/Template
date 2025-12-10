namespace Template.Client.Features.Auth.State;

// Acción para solicitar la generación de password.
#pragma warning disable SA1649
public class GeneratePasswordAction(int length, bool useNumbers, bool useSpecial)
#pragma warning restore SA1649
{
	public int Length { get; } = length;

	public bool UseNumbers { get; } = useNumbers;

	public bool UseSpecial { get; } = useSpecial;
}

// Acción que se dispara cuando la contraseña ha sido generada.
#pragma warning disable SA1402
public class PasswordGeneratedAction(string globalPassword)
#pragma warning restore SA1402
{
	public string GlobalPassword { get; } = globalPassword;
}

// Acción para iniciar sesión.
#pragma warning disable SA1402
public class LoginAction { }
#pragma warning restore SA1402

// Acción cuando el login es exitoso.
#pragma warning disable SA1402
public class LoginSuccessAction(string token)
#pragma warning restore SA1402
{
	public string Token { get; } = token;
}

// Acción cuando el login falla.
#pragma warning disable SA1402
public class LoginFailureAction(string error)
#pragma warning restore SA1402
{
	public string Error { get; } = error;
}
