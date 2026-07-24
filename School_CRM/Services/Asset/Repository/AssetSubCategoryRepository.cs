using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetSubCategoryRepository : IAssetSubCategoryRepository
    {
        private readonly LibmanagementContext _db;
        public AssetSubCategoryRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmSubCategory>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.AsmSubCategories.Include(x => x.Category).AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.SubCategoryName).ToListAsync();
        }

        public async Task<List<AsmSubCategory>> GetByCategoryAsync(int categoryId) =>
            await _db.AsmSubCategories
                .Where(x => x.CategoryId == categoryId && x.IsActive)
                .OrderBy(x => x.SubCategoryName)
                .ToListAsync();

        public async Task<AsmSubCategory?> GetByIdAsync(int id) =>
            await _db.AsmSubCategories.Include(x => x.Category).FirstOrDefaultAsync(x => x.SubCategoryId == id);

        public async Task<bool> ExistsAsync(int categoryId, string name, int excludeId = 0) =>
            await _db.AsmSubCategories.AnyAsync(x =>
                x.CategoryId == categoryId && x.SubCategoryName == name && x.SubCategoryId != excludeId);

        public async Task<AsmSubCategory> CreateAsync(AsmSubCategory entity)
        {
            _db.AsmSubCategories.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmSubCategory> UpdateAsync(AsmSubCategory entity)
        {
            _db.AsmSubCategories.Update(entity);
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
