using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetMasterRepository : IAssetMasterRepository
    {
        private readonly LibmanagementContext _db;
        public AssetMasterRepository(LibmanagementContext db) => _db = db;

        public async Task<(List<AssetListItemDto> Items, int TotalCount)> SearchAsync(AssetSearchDto filter)
        {
            var q = _db.AsmAssets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Where(x => x.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var s = filter.SearchText.Trim().ToLower();
                q = q.Where(x => x.AssetName.ToLower().Contains(s)
                               || x.AssetCode.ToLower().Contains(s)
                               || (x.Brand != null && x.Brand.ToLower().Contains(s)));
            }
            if (filter.CategoryId.HasValue && filter.CategoryId > 0)
                q = q.Where(x => x.CategoryId == filter.CategoryId);
            if (filter.SubCategoryId.HasValue && filter.SubCategoryId > 0)
                q = q.Where(x => x.SubCategoryId == filter.SubCategoryId);
            if (filter.OnlyAvailable == true)
                q = q.Where(x => x.AvailableUnits > 0);

            var total = await q.CountAsync();
            var items = await q
                .OrderBy(x => x.AssetName)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x => new AssetListItemDto
                {
                    AssetId        = x.AssetId,
                    AssetCode      = x.AssetCode,
                    AssetName      = x.AssetName,
                    CategoryName   = x.Category.CategoryName,
                    SubCategoryName = x.SubCategory != null ? x.SubCategory.SubCategoryName : null,
                    Brand          = x.Brand,
                    TotalUnits     = x.TotalUnits,
                    AvailableUnits = x.AvailableUnits,
                    UnitPrice      = x.UnitPrice,
                    IsIssuable     = x.IsIssuable,
                    IsActive       = x.IsActive
                })
                .ToListAsync();

            return (items, total);
        }

        public async Task<AsmAsset?> GetByIdAsync(int id) =>
            await _db.AsmAssets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .FirstOrDefaultAsync(x => x.AssetId == id);

        public async Task<AsmAsset?> GetByIdWithUnitsAsync(int id) =>
            await _db.AsmAssets
                .Include(x => x.Category)
                .Include(x => x.SubCategory)
                .Include(x => x.AsmAssetUnits)
                .ThenInclude(u => u.CurrentLocation)
                .FirstOrDefaultAsync(x => x.AssetId == id);

        public async Task<AsmAsset> CreateAsync(AsmAsset entity)
        {
            _db.AsmAssets.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmAsset> UpdateAsync(AsmAsset entity)
        {
            _db.AsmAssets.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeactivateAsync(int id, int updatedBy)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            entity.IsActive   = false;
            entity.UpdatedAt  = DateTime.Now;
            entity.UpdatedBy  = updatedBy;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateAssetCodeAsync(string categoryCode, int year)
        {
            var prefix = $"ASM-{categoryCode.ToUpper()}-{year}-";
            var count  = await _db.AsmAssets
                .CountAsync(x => x.AssetCode.StartsWith(prefix));
            return $"{prefix}{(count + 1):D3}";
        }

        public async Task<bool> UpdateUnitCountsAsync(int assetId, int totalDelta, int availableDelta)
        {
            var asset = await _db.AsmAssets.FindAsync(assetId);
            if (asset == null) return false;
            asset.TotalUnits     = Math.Max(0, asset.TotalUnits + totalDelta);
            asset.AvailableUnits = Math.Max(0, asset.AvailableUnits + availableDelta);
            asset.UpdatedAt      = DateTime.Now;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
