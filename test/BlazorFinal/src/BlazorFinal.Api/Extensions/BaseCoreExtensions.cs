using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorFinal.Api.Extensions;

public static class FrameworkExtensions
{
    public static IServiceCollection AddBaseCoreWeb(this IServiceCollection services, IConfiguration configuration)
    {
        // Placeholder for BaseCore Web registration
        return services;
    }

    public static void EnsureCreated(this WebApplication app)
    {
        // Placeholder for migration logic
    }
}
