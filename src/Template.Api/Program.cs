using Autofac;
using Autofac.Extensions.DependencyInjection;

using BaseCore.Framework.DependencyInjection.Container;
using BaseCore.Framework.Observability.Logging;
using BaseCore.Framework.Observability.Logging.Interfaces;
using BaseCore.Framework.Security.Identity;

using Template.Application;
using Template.Infrastructure;
using Template.Infrastructure.Configuration;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("Configuration/basecore.applicationsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationLayer();
builder.Services.AddInfrastructureLayer(builder.Configuration);

// Configure Autofac
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
	ApplicationSettings? settings = new();
	builder.Configuration.Bind(settings);

	BaseCoreContainerBuilder baseCoreBuilder = new(containerBuilder);
	BaseCore.Framework.Observability.Logging.IoC.ClientLoggingDependencyInjection.RegisterDependencyInjection(baseCoreBuilder, settings);

	containerBuilder.RegisterType<Logger>().As<IBaseCoreLogger>().InstancePerLifetimeScope();
});

// Auth Validation
builder.Services.AddBaseCoreIdentityValidation();

// End of services

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<BaseCore.Framework.Web.Middlewares.ExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();
