using BaseCore.Framework.Security.Identity.Configuration;

using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.Context;

// This context now inherits from BaseCoreIdentityDbContext which is IdentityDbContext<BaseUser>
public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : BaseCoreIdentityDbContext(options)
{
}
