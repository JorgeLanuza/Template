using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VerificationProject.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // Register EF Core, Repositories, etc.
        // services.AddDbContext<ApplicationDbContext>(...);
        return services;
    }
}
