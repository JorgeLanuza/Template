namespace Template.Client.Features.Admin.Models;

public class AdminUserModel
{
	public string Id { get; set; } = string.Empty;

	public string UserName { get; set; } = string.Empty;

	public string Email { get; set; } = string.Empty;

	public bool IsLocked { get; set; }

	public DateTime CreatedAt { get; set; }

	public List<string> Roles { get; set; } = [];

	public DateTime? LastActive { get; set; }

	public string LastIp { get; set; } = string.Empty;
}
