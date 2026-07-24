using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetVendorRepository : IAssetVendorRepository
    {
        private readonly LibmanagementContext _db;
        public AssetVendorRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmVendor>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.AsmVendors.AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.VendorName).ToListAsync();
        }

        public async Task<AsmVendor?> GetByIdAsync(int id) =>
            await _db.AsmVendors.FindAsync(id);

        public async Task<AsmVendor> CreateAsync(AsmVendor entity)
        {
            _db.AsmVendors.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmVendor> UpdateAsync(AsmVendor entity)
        {
            _db.AsmVendors.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
