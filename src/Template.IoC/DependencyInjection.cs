using System.Reflection;

using Autofac;

using AutoMapper;

using BaseCore.Framework.Configuration.ApplicationSettings;
using BaseCore.Framework.DependencyInjection.Container;
using BaseCore.Framework.IdentityServer.Configuration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Template.Infrastructure.Context;

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
	}

	public static void RegisterDependencyInjector(BaseCoreContainerBuilder builder)
	{
		builder.Register(context =>
		{
			var loggerFactory = context.Resolve<Microsoft.Extensions.Logging.ILoggerFactory>();
			var expression = new MapperConfigurationExpression();
			expression.AddMaps(Assembly.GetExecutingAssembly());
			var config = new MapperConfiguration(expression, loggerFactory);
			return config.CreateMapper();
		}).As<IMapper>().InstancePerLifetimeScope();
	}

	public static void EnsureDatabaseCreated(IServiceProvider serviceProvider)
	{
		using IServiceScope scope = serviceProvider.CreateScope();
		IServiceProvider services = scope.ServiceProvider;

		AppDbContext context = services.GetRequiredService<AppDbContext>();
		context.Database.EnsureCreated();

		AppIdentityDbContext identityContext = services.GetRequiredService<AppIdentityDbContext>();
		identityContext.Database.EnsureCreated();
	}
}
