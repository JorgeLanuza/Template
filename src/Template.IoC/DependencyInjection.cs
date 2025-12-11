using System.Reflection;

using Autofac;

using AutoMapper;

using BaseCore.Framework.Configuration.ApplicationSettings;
using BaseCore.Framework.DependencyInjection.Container;
using BaseCore.Framework.IdentityServer.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OpenIddict.Abstractions;

using Template.Infrastructure.Context;
using AppIdentityDbContext = Template.Infrastructure.Context.AppIdentityDbContext;

namespace Template.IoC;

public static class DependencyInjection
{
	public static void RegisterServices(IServiceCollection services, IConfiguration configuration)
	{
		BaseCoreApplicationSettings baseCoreSettings = new();
		configuration.Bind(baseCoreSettings);
		string? connectionString = baseCoreSettings.ConnectionStrings.FirstOrDefault(x => x.Default = true)?.EncodedString ?? baseCoreSettings.ConnectionStrings.FirstOrDefault()?.EncodedString;

		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlServer(connectionString));

		services.AddDbContext<AppIdentityDbContext>(options =>
			options.UseSqlServer(connectionString));

		// Registrar SecurityDbContext (Identity)
		services.AddDbContext<BaseCore.Framework.Security.DataAccess.Context.SecurityDbContext>(options =>
			options.UseSqlServer(baseCoreSettings.ConnectionStrings.FirstOrDefault(x => x.Name == "ATHConnection")?.EncodedString ?? connectionString));

		// Registrar Repositorios de Seguridad
		services.AddScoped<BaseCore.Framework.Security.DataAccess.Repositories.Authentication.AuthenticationRepository>();
		services.AddScoped<BaseCore.Framework.Security.DataAccess.Repositories.Authentication.UserRepository>();
		services.AddScoped<BaseCore.Framework.Security.DataAccess.Repositories.Authorization.RoleRepository>();
		services.AddScoped<BaseCore.Framework.Security.DataAccess.Repositories.Authorization.UserApplicationRolesRepository>();

		// Registrar Servicios de Negocio de Seguridad (Framework 1.0.2)
		services.AddScoped<Microsoft.AspNetCore.Identity.IPasswordHasher<BaseCore.Framework.Security.DataAccess.Entities.Authentication.AuthenticationEntity>, Microsoft.AspNetCore.Identity.PasswordHasher<BaseCore.Framework.Security.DataAccess.Entities.Authentication.AuthenticationEntity>>();
		services.AddScoped<BaseCore.Framework.Security.Business.Services.CredentialPoliciesManagerService>();
		services.AddScoped<BaseCore.Framework.Security.Business.Services.AuthenticationService>();
		services.AddScoped<BaseCore.Framework.Security.Business.Services.CredentialManagerService>();

		// Dependencias de AuthenticationService
		services.AddHttpContextAccessor();
		services.AddScoped<BaseCore.Framework.Security.Business.Helpers.HttpContextHelper>();
		services.AddScoped<BaseCore.Framework.Observability.Audit.BaseCoreAuditFactory>();
		services.AddScoped(typeof(BaseCore.Framework.Domain.Validations.BaseCoreValidation<>));
	}

	public static void RegisterDependencyInjector(BaseCoreContainerBuilder builder)
	{
		builder.Register(context =>
		{
			ILoggerFactory loggerFactory = context.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>();
			MapperConfigurationExpression expression = new();
			expression.AddMaps(Assembly.GetExecutingAssembly());
			MapperConfiguration config = new(expression, loggerFactory);
			return config.CreateMapper();
		}).As<IMapper>().InstancePerLifetimeScope();
	}
}
