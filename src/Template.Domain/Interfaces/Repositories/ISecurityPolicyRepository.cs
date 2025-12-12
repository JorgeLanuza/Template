using BaseCore.Framework.Security.Identity.Entities;

namespace Template.Domain.Interfaces.Repositories;

public interface ISecurityPolicyRepository
{
	Task<PasswordPolicyEntity?> GetPasswordPolicyAsync();

	Task UpdatePasswordPolicyAsync(PasswordPolicyEntity policy);

	Task<LockingPolicyEntity?> GetLockingPolicyAsync();

	Task UpdateLockingPolicyAsync(LockingPolicyEntity policy);
}
