using BaseCore.Framework.Infrastructure.Context;

using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.Context;

public class AppDbContext(DbContextOptions<AppDbContext> options) : BaseCoreContext(options)
{
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		ConfigureDB(modelBuilder);
		base.OnModelCreating(modelBuilder);
	}

	private static void ConfigureDB(ModelBuilder modelBuilder)
	{
		// Method intentionally left empty waiting for your bussiness setup.
	}
}
