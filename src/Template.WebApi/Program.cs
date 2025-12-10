using Autofac;
using Scalar.AspNetCore;
using Autofac.Extensions.DependencyInjection;

using BaseCore.Framework.Security.Identity;
using BaseCore.Framework.Security.Identity;
using BaseCore.Framework.Security.DataAccess.Context;
using BaseCore.Framework.Web.Middlewares;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Core;
using Template.Application;
using Template.DependencyInjection.Container;

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

// Configurar Identity Server (Self-Hosted)
builder.Services.AddBaseCoreIdentityServer<SecurityDbContext>(options =>
{
    // Configure OpenIddict to use EF Core
    var coreBuilder = (OpenIddictCoreBuilder)options;
    coreBuilder.UseEntityFrameworkCore()
               .UseDbContext<SecurityDbContext>();
});

// Habilitar Validación de Tokens (Local) - Resource Server
builder.Services.AddBaseCoreIdentityValidation();

WebApplication app = builder.Build();

// Configurar el pipeline de solicitudes HTTP
app.UseMiddleware<ExceptionMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
