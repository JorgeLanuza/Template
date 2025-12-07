using BaseCore.Framework.Security.Identity.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.Identity;

public class AppDbContext(DbContextOptions<AppDbContext> options) : BaseCoreIdentityDbContext(options)
{
	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
	}
}
