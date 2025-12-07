using Microsoft.AspNetCore.Identity;

namespace Template.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    // Add custom profiles data here
    public string FullName { get; set; } = string.Empty;
}
