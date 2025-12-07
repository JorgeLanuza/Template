using Template.Infrastructure.Identity;

namespace Template.Web.Extensions;

public static class AppExtensions
{
	public static void EnsureCreated(this WebApplication app)
	{
		using IServiceScope scope = app.Services.CreateScope();
		AppDbContext context = scope.ServiceProvider.GetRequiredService<Template.Infrastructure.Identity.AppDbContext>();

		context.Database.EnsureCreated();
	}
}
