using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetCategoryRepository : IAssetCategoryRepository
    {
        private readonly LibmanagementContext _db;
        public AssetCategoryRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmCategory>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.AsmCategories.AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.CategoryName).ToListAsync();
        }

        public async Task<AsmCategory?> GetByIdAsync(int id) =>
            await _db.AsmCategories.FindAsync(id);

        public async Task<bool> ExistsAsync(string name, int excludeId = 0) =>
            await _db.AsmCategories.AnyAsync(x => x.CategoryName == name && x.CategoryId != excludeId);

        public async Task<AsmCategory> CreateAsync(AsmCategory entity)
        {
            _db.AsmCategories.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmCategory> UpdateAsync(AsmCategory entity)
        {
            _db.AsmCategories.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            var hasAssets = await _db.AsmAssets.AnyAsync(x => x.CategoryId == id && x.IsActive);
            if (hasAssets) return false;
            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
