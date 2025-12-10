using Template.Client.Features.Auth.Services;
using Template.Client.Features.Auth.Services.Interfaces;
using Template.Client.Features.Auth.State;
using Template.Client.State;
using Microsoft.AspNetCore.Components.Authorization;

namespace Template.Client.Extensions;

// Extension methods for registering Client-side services.
public static class ServiceCollectionExtensions
{
	// Registra todos los servicios, estados y componentes específicos de la aplicación Cliente (Blazor).
	// Guía para el equipo:
	// Usa este método para registrar cualquier cosa que sea exclusiva de la capa visual/cliente. Por ejemplo: Stores de estado, ViewModels, wrappers de IJsRuntime, etc.
	// Importante: No registres aquí infraestructura pesada (Repositorios, DbContexts). Esos deben ir en los proyectos de IoC o Infrastructure para mantener la arquitectura limpia.
	public static IServiceCollection AddClientServices(this IServiceCollection services)
	{
		// Gestión de Estado (Flux Pattern)
		// Registramos el Store central y nos aseguramos de inicializar los Reducers para que empiecen a escuchar eventos desde el arranque.
		services.AddSingleton<Store>(sp =>
		{
			Store store = new();

			// Inicializa aquí los Reducers de tus Features
			AuthReducers.Initialize(store);

			return store;
		});

		// Servicios de UI y ViewModels
		// Registra aquí tus servicios scoped/transient necesarios para los componentes.
		// Configure HttpClient with Backend Base Address
		services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7197") });

		services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
		services.AddScoped<IAuthService, AuthService>();

		return services;
	}
}
