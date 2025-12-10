using BaseCore.Framework.Configuration.ApplicationSettings;
using BaseCore.Framework.DependencyInjection.Container;
using BaseCore.Framework.Observability.Logging;
using BaseCore.Framework.Observability.Logging.Interfaces;
using BaseCore.Framework.Observability.Logging.IoC;

using Microsoft.Extensions.Configuration;

namespace Template.DependencyInjection.Container;

public class TemplateContainerBuilder(Autofac.ContainerBuilder builder, IConfiguration configuration) : BaseCoreContainerBuilder(builder)
{
	private readonly IConfiguration _configuration = configuration;

	public void RegisterModule()
	{
		BaseCoreApplicationSettings settings = new();
		_configuration.Bind(settings);

		ClientLoggingDependencyInjection.RegisterDependencyInjection(this, settings);

		Template.IoC.DependencyInjection.RegisterDependencyInjector(this);

		RegisterType<Logger>().As<IBaseCoreLogger>().InstancePerLifetimeScope();
	}
}
