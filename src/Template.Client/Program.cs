using Autofac;
using Autofac.Extensions.DependencyInjection;

using MudBlazor.Services;

using Template.Client;
using Template.DependencyInjection.Container;

string configFilePath = "Core/Configuration/BaseCore.ApplicationSettings.json";

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);

builder.Services.AddRazorComponents()
	.AddInteractiveServerComponents();

builder.Services.AddMudServices();

Template.IoC.DependencyInjection.RegisterServices(builder.Services, builder.Configuration);

// Configure Autofac Container
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
	var templateBuilder = new TemplateContainerBuilder(containerBuilder, builder.Configuration);
	templateBuilder.RegisterModule();
});

WebApplication? app = builder.Build();

// Initialize Database
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
