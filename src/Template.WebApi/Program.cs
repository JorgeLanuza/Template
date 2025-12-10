using Autofac;
using Scalar.AspNetCore;
using Autofac.Extensions.DependencyInjection;

using BaseCore.Framework.Security.Identity;
using BaseCore.Framework.Web.Middlewares;
using Template.Application;
using Template.DependencyInjection.Container;

string configFilePath = "Configuration/BaseCore.ApplicationSettings.json";

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(configFilePath, optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddApplicationLayer();

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
	TemplateContainerBuilder templateBuilder = new(containerBuilder, builder.Configuration);
	templateBuilder.RegisterModule();
});

// Auth Validation
builder.Services.AddBaseCoreIdentityValidation();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionMiddleware>();

app.MapOpenApi();
app.MapScalarApiReference();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
