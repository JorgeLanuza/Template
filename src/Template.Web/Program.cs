using Autofac;
using Autofac.Extensions.DependencyInjection;

using BaseCore.Framework.Configuration.ApplicationSettings;

using Microsoft.EntityFrameworkCore;

using MudBlazor.Services;

using Template.Client.Components;
using Template.Client.Extensions;
using Template.Infrastructure.Context;


WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// RESTAURADO: Carga la configuración de BaseCore
builder.Configuration.AddJsonFile("Configuration/BaseCore.ApplicationSettings.json", optional: false, reloadOnChange: true);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddMudServices();

// Registrar servicios del Cliente (Store, etc.)
builder.Services.AddClientServices();

// Registrar servicios del Template (DbContexts, etc.)
Template.IoC.DependencyInjection.RegisterServices(builder.Services, builder.Configuration);

// Configurar contenedor Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
	var templateBuilder = new Template.DependencyInjection.ContainerBuilder.TemplateContainerBuilder(containerBuilder, builder.Configuration);
	templateBuilder.RegisterModule();
});

WebApplication? app = builder.Build();

// Inicializar Base de Datos
Template.IoC.DependencyInjection.EnsureDatabaseCreated(app.Services);

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error", createScopeForErrors: true);
	app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

await app.RunAsync();
