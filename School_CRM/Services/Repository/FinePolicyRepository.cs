using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class FinePolicyRepository : IFinePolicyRepository
    {
        private readonly LibmanagementContext _context;

        public FinePolicyRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<LibFinePolicy?> GetActivePolicyAsync()
        {
            return await _context.LibFinePolicies
                .FirstOrDefaultAsync(x => x.IsActive);
        }

        public async Task<LibFinePolicy?> GetByIdAsync(int id)
        {
            return await _context.LibFinePolicies
                .FirstOrDefaultAsync(x => x.PolicyId == id);
        }

        public async Task<List<LibFinePolicy>> GetAllAsync()
        {
            return await _context.LibFinePolicies
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<LibFinePolicy> CreateAsync(LibFinePolicy policy)
        {
            _context.LibFinePolicies.Add(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task DeactivateAllAsync()
        {
            var policies = await _context.LibFinePolicies
                .Where(x => x.IsActive)
                .ToListAsync();

            foreach (var policy in policies)
            {
                policy.IsActive = false;
            }

            await _context.SaveChangesAsync();
        }
    }
}
