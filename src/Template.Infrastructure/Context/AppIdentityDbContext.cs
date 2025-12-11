using BaseCore.Framework.Security.Identity.Configuration;

using Microsoft.EntityFrameworkCore;

namespace Template.Infrastructure.Context;

public class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options) : BaseCoreIdentityDbContext(options)
{
}
