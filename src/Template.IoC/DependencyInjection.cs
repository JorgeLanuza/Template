using System.Reflection;

using Autofac;

using AutoMapper;

using BaseCore.Framework.Configuration.ApplicationSettings;
using BaseCore.Framework.DependencyInjection.Container;
using BaseCore.Framework.Domain.Validations;
using BaseCore.Framework.Observability.Audit;
using BaseCore.Framework.Security.Business.Helpers;
using BaseCore.Framework.Security.Business.Services;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

		services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

		services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(connectionString));

		services.AddScoped<BaseCore.Framework.Security.Identity.Configuration.BaseCoreIdentityDbContext>(provider => provider.GetRequiredService<AppIdentityDbContext>());

		services.AddScoped<IdentityAuthenticationService>();
		services.AddScoped<IdentityUserService>();
		services.AddScoped<IdentityRoleService>();

		services.AddScoped<CredentialPoliciesManagerService>();

		services.AddHttpContextAccessor();
		services.AddScoped<HttpContextHelper>();
		services.AddScoped<BaseCoreAuditFactory>();
		services.AddScoped(typeof(BaseCoreValidation<>));
	}

	public static void RegisterDependencyInjector(BaseCoreContainerBuilder builder)
	{
		builder.Register(context =>
		{
			ILoggerFactory loggerFactory = context.Resolve<ILoggerFactory>();
			MapperConfigurationExpression expression = new();
			expression.AddMaps(Assembly.GetExecutingAssembly());
			MapperConfiguration config = new(expression, loggerFactory);
			return config.CreateMapper();
		}).As<IMapper>().InstancePerLifetimeScope();
	}
}
