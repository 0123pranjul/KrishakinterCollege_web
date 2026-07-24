using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetLocationRepository : IAssetLocationRepository
    {
        private readonly LibmanagementContext _db;
        public AssetLocationRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmLocation>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.AsmLocations.AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.LocationName).ToListAsync();
        }

        public async Task<AsmLocation?> GetByIdAsync(int id) =>
            await _db.AsmLocations.FindAsync(id);

        public async Task<bool> ExistsAsync(string name, int excludeId = 0) =>
            await _db.AsmLocations.AnyAsync(x => x.LocationName == name && x.LocationId != excludeId);

        public async Task<AsmLocation> CreateAsync(AsmLocation entity)
        {
            _db.AsmLocations.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmLocation> UpdateAsync(AsmLocation entity)
        {
            _db.AsmLocations.Update(entity);
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
