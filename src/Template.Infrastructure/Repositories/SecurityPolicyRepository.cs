using BaseCore.Framework.Security.Identity.Entities;

using Microsoft.EntityFrameworkCore;

using Template.Domain.Interfaces.Repositories;
using Template.Infrastructure.Context;

namespace Template.Infrastructure.Repositories;

public class SecurityPolicyRepository(AppIdentityDbContext context) : ISecurityPolicyRepository
{
	private readonly AppIdentityDbContext _context = context;

	public async Task<PasswordPolicyEntity?> GetPasswordPolicyAsync()
	{
		return await _context.PasswordPolicies.FirstOrDefaultAsync();
	}

	public async Task UpdatePasswordPolicyAsync(PasswordPolicyEntity policy)
	{
		if (policy.Id == 0) // New entity
		{
			_context.PasswordPolicies.Add(policy);
		}
		else
		{
			_context.PasswordPolicies.Update(policy);
		}
		await _context.SaveChangesAsync();
	}

	public async Task<LockingPolicyEntity?> GetLockingPolicyAsync()
	{
		return await _context.LockingPolicies.FirstOrDefaultAsync();
	}

	public async Task UpdateLockingPolicyAsync(LockingPolicyEntity policy)
	{
		if (policy.Id == 0)
		{
			_context.LockingPolicies.Add(policy);
		}
		else
		{
			_context.LockingPolicies.Update(policy);
		}
		await _context.SaveChangesAsync();
	}
}
