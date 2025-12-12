using BaseCore.Framework.Security.Identity.Dtos;

namespace Template.Client.Features.Admin.Services;

public interface IAdminService
{
    Task<PasswordPolicyDto?> GetPasswordPolicyAsync();
    Task<bool> UpdatePasswordPolicyAsync(PasswordPolicyDto policy);
    Task<LockingPolicyDto?> GetLockingPolicyAsync();
    Task<bool> UpdateLockingPolicyAsync(LockingPolicyDto policy);
}
