using Microsoft.Extensions.DependencyInjection;

namespace BlazorTest.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        // Register MediatR, Validators, etc.
        // services.AddMediatR(...);
        return services;
    }
}
