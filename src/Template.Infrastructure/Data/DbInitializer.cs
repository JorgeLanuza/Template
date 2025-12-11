using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using OpenIddict.Abstractions;

using Template.Infrastructure.Context;

namespace Template.Infrastructure.Data;

public static class DbInitializer
{
	public static void EnsureDatabaseCreated(IServiceProvider serviceProvider)
	{
		using IServiceScope scope = serviceProvider.CreateScope();
		IServiceProvider services = scope.ServiceProvider;

		AppDbContext context = services.GetRequiredService<AppDbContext>();
		context.Database.EnsureCreated();

		AppIdentityDbContext identityContext = services.GetRequiredService<AppIdentityDbContext>();
		identityContext.Database.EnsureCreated();
	}

	public static async Task SeedOpenIddictClientsAsync(IServiceProvider serviceProvider, IConfiguration configuration)
	{
		using IServiceScope scope = serviceProvider.CreateScope();
		IOpenIddictApplicationManager manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

		string? clientId = configuration["IdentityServerSettings:ClientSettings:ClientId"];

		if (string.IsNullOrEmpty(clientId))
			return;

		if (await manager.FindByClientIdAsync(clientId) is null)
		{
			await manager.CreateAsync(new OpenIddictApplicationDescriptor
			{
				ClientId = clientId,
				DisplayName = "BaseCore Client",
				Permissions =
				{
					OpenIddictConstants.Permissions.Endpoints.Token,
					OpenIddictConstants.Permissions.Endpoints.Authorization,
					OpenIddictConstants.Permissions.GrantTypes.Password,
					OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
					OpenIddictConstants.Permissions.Prefixes.Scope + "openid",
					OpenIddictConstants.Permissions.Prefixes.Scope + "profile",
					OpenIddictConstants.Permissions.Prefixes.Scope + "email",
					OpenIddictConstants.Permissions.Prefixes.Scope + "offline_access",
					OpenIddictConstants.Permissions.Prefixes.Scope + "api1",
				},
			});
		}
	}
}
