using Autofac;
using Autofac.Extensions.DependencyInjection;

using BaseCore.Framework.Security.Identity;
using BaseCore.Framework.Web.Middlewares;
using BaseCore.Framework.Security.Identity.Entities;
using Microsoft.AspNetCore.Identity;

using Scalar.AspNetCore;

using Template.Application;
using Template.DependencyInjection.Container;
using Template.Infrastructure.Data;

using AppIdentityDbContext = Template.Infrastructure.Context.AppIdentityDbContext;

string configFilePath = "Configuration/BaseCore.ApplicationSettings.json";

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddApplicationLayer();

// Registra Servicios de IoC (DbContext, Repositories)
Template.IoC.DependencyInjection.RegisterServices(builder.Services, builder.Configuration);

// Configurar Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
	TemplateContainerBuilder templateBuilder = new(containerBuilder, builder.Configuration);
	templateBuilder.RegisterModule();
});

builder.Services.AddIdentityCore<BaseUser>().AddRoles<BaseRole>().AddEntityFrameworkStores<AppIdentityDbContext>().AddSignInManager().AddDefaultTokenProviders();

builder.Services.AddBaseCoreIdentityServer<AppIdentityDbContext>(options =>
{
	OpenIddictCoreBuilder coreBuilder = (OpenIddictCoreBuilder)options;
	coreBuilder.UseEntityFrameworkCore().UseDbContext<AppIdentityDbContext>();
});

builder.Services.AddBaseCoreIdentityValidation();

WebApplication app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.UseAuthorization();
app.MapControllers();

DbInitializer.EnsureDatabaseCreated(app.Services);

await DbInitializer.SeedOpenIddictClientsAsync(app.Services, app.Configuration);

await app.RunAsync();
