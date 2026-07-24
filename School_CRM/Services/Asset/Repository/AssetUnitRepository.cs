using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetUnitRepository : IAssetUnitRepository
    {
        private readonly LibmanagementContext _db;
        public AssetUnitRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmAssetUnit>> GetByAssetIdAsync(int assetId) =>
            await _db.AsmAssetUnits
                .Include(x => x.CurrentLocation)
                .Include(x => x.Vendor)
                .Where(x => x.AssetId == assetId && x.IsActive)
                .OrderBy(x => x.AssetTag)
                .ToListAsync();

        public async Task<AsmAssetUnit?> GetByIdAsync(int unitId) =>
            await _db.AsmAssetUnits
                .Include(x => x.Asset).ThenInclude(a => a.Category)
                .Include(x => x.CurrentLocation)
                .Include(x => x.Vendor)
                .FirstOrDefaultAsync(x => x.UnitId == unitId);

        public async Task<AsmAssetUnit?> GetByTagAsync(string assetTag) =>
            await _db.AsmAssetUnits
                .Include(x => x.Asset).ThenInclude(a => a.Category)
                .Include(x => x.Asset).ThenInclude(a => a.SubCategory)
                .Include(x => x.CurrentLocation)
                .Include(x => x.Vendor)
                .FirstOrDefaultAsync(x => x.AssetTag == assetTag && x.IsActive);

        public async Task<string> GenerateAssetTagAsync(int year)
        {
            var prefix = $"ASM-{year}-";
            var count  = await _db.AsmAssetUnits
                .CountAsync(x => x.AssetTag.StartsWith(prefix));
            return $"{prefix}{(count + 1):D5}";
        }

        public async Task<AsmAssetUnit> CreateAsync(AsmAssetUnit entity)
        {
            _db.AsmAssetUnits.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmAssetUnit> UpdateAsync(AsmAssetUnit entity)
        {
            _db.AsmAssetUnits.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> UpdateAvailabilityAsync(int unitId, bool isAvailable,
            string? condition = null, string? assignedToType = null,
            int? assignedToId = null, int? locationId = null)
        {
            var unit = await _db.AsmAssetUnits.FindAsync(unitId);
            if (unit == null) return false;

            unit.IsAvailable = isAvailable;
            if (condition != null)       unit.UnitCondition   = condition;
            if (assignedToType != null)  unit.AssignedToType  = assignedToType;
            if (assignedToId.HasValue)   unit.AssignedToId    = assignedToId;
            if (locationId.HasValue)     unit.CurrentLocationId = locationId;

            await _db.SaveChangesAsync();
            return true;
        }
    }
}
