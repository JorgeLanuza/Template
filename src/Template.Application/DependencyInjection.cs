using Microsoft.Extensions.DependencyInjection;

namespace Template.Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
	{
		// Register validators, etc.

		return services;
	}
}
