using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset
{
    // ── Category ─────────────────────────────────────────────────────────────
    public class AssetCategoryService : IAssetCategoryService
    {
        private readonly IAssetCategoryRepository _repo;
        public AssetCategoryService(IAssetCategoryRepository repo) => _repo = repo;

        public Task<List<AsmCategory>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<AsmCategory?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(AssetCategoryDto dto, int createdBy)
        {
            if (await _repo.ExistsAsync(dto.CategoryName))
                return (false, $"Category '{dto.CategoryName}' already exists.");

            await _repo.CreateAsync(new AsmCategory
            {
                CategoryName = dto.CategoryName.Trim(),
                Description  = dto.Description?.Trim(),
                IsActive     = dto.IsActive,
                CreatedAt    = DateTime.Now,
                CreatedBy    = createdBy
            });
            return (true, "Category created successfully.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(AssetCategoryDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.CategoryId);
            if (entity == null) return (false, "Category not found.");
            if (await _repo.ExistsAsync(dto.CategoryName, dto.CategoryId))
                return (false, $"Category '{dto.CategoryName}' already exists.");

            entity.CategoryName = dto.CategoryName.Trim();
            entity.Description  = dto.Description?.Trim();
            entity.IsActive     = dto.IsActive;
            await _repo.UpdateAsync(entity);
            return (true, "Category updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? (true, "Category deactivated.") : (false, "Cannot delete — has active assets or not found.");
        }
    }

    // ── SubCategory ───────────────────────────────────────────────────────────
    public class AssetSubCategoryService : IAssetSubCategoryService
    {
        private readonly IAssetSubCategoryRepository _repo;
        public AssetSubCategoryService(IAssetSubCategoryRepository repo) => _repo = repo;

        public Task<List<AsmSubCategory>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<List<AsmSubCategory>> GetByCategoryAsync(int categoryId) => _repo.GetByCategoryAsync(categoryId);
        public Task<AsmSubCategory?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(AssetSubCategoryDto dto, int createdBy)
        {
            if (await _repo.ExistsAsync(dto.CategoryId, dto.SubCategoryName))
                return (false, $"Sub-category '{dto.SubCategoryName}' already exists in this category.");

            await _repo.CreateAsync(new AsmSubCategory
            {
                CategoryId      = dto.CategoryId,
                SubCategoryName = dto.SubCategoryName.Trim(),
                Description     = dto.Description?.Trim(),
                IsActive        = dto.IsActive,
                CreatedAt       = DateTime.Now,
                CreatedBy       = createdBy
            });
            return (true, "Sub-category created.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(AssetSubCategoryDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.SubCategoryId);
            if (entity == null) return (false, "Sub-category not found.");
            if (await _repo.ExistsAsync(dto.CategoryId, dto.SubCategoryName, dto.SubCategoryId))
                return (false, "Name already exists in this category.");

            entity.CategoryId      = dto.CategoryId;
            entity.SubCategoryName = dto.SubCategoryName.Trim();
            entity.Description     = dto.Description?.Trim();
            entity.IsActive        = dto.IsActive;
            await _repo.UpdateAsync(entity);
            return (true, "Sub-category updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? (true, "Sub-category deactivated.") : (false, "Not found.");
        }
    }

    // ── Location ──────────────────────────────────────────────────────────────
    public class AssetLocationService : IAssetLocationService
    {
        private readonly IAssetLocationRepository _repo;
        public AssetLocationService(IAssetLocationRepository repo) => _repo = repo;

        public Task<List<AsmLocation>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<AsmLocation?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(AssetLocationDto dto, int createdBy)
        {
            if (await _repo.ExistsAsync(dto.LocationName))
                return (false, $"Location '{dto.LocationName}' already exists.");

            await _repo.CreateAsync(new AsmLocation
            {
                LocationName = dto.LocationName.Trim(),
                LocationType = dto.LocationType,
                Floor        = dto.Floor?.Trim(),
                Building     = dto.Building?.Trim(),
                IsActive     = dto.IsActive,
                CreatedAt    = DateTime.Now,
                CreatedBy    = createdBy
            });
            return (true, "Location created.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(AssetLocationDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.LocationId);
            if (entity == null) return (false, "Location not found.");
            if (await _repo.ExistsAsync(dto.LocationName, dto.LocationId))
                return (false, "Location name already exists.");

            entity.LocationName = dto.LocationName.Trim();
            entity.LocationType = dto.LocationType;
            entity.Floor        = dto.Floor?.Trim();
            entity.Building     = dto.Building?.Trim();
            entity.IsActive     = dto.IsActive;
            await _repo.UpdateAsync(entity);
            return (true, "Location updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? (true, "Location deactivated.") : (false, "Not found.");
        }
    }

    // ── Vendor ────────────────────────────────────────────────────────────────
    public class AssetVendorService : IAssetVendorService
    {
        private readonly IAssetVendorRepository _repo;
        public AssetVendorService(IAssetVendorRepository repo) => _repo = repo;

        public Task<List<AsmVendor>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<AsmVendor?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(AssetVendorDto dto, int createdBy)
        {
            await _repo.CreateAsync(new AsmVendor
            {
                VendorName    = dto.VendorName.Trim(),
                ContactPerson = dto.ContactPerson?.Trim(),
                Phone         = dto.Phone?.Trim(),
                Email         = dto.Email?.Trim(),
                Address       = dto.Address?.Trim(),
                Gstno         = dto.GSTNo?.Trim(),
                IsActive      = dto.IsActive,
                CreatedAt     = DateTime.Now,
                CreatedBy     = createdBy
            });
            return (true, "Vendor created.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(AssetVendorDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.VendorId);
            if (entity == null) return (false, "Vendor not found.");

            entity.VendorName    = dto.VendorName.Trim();
            entity.ContactPerson = dto.ContactPerson?.Trim();
            entity.Phone         = dto.Phone?.Trim();
            entity.Email         = dto.Email?.Trim();
            entity.Address       = dto.Address?.Trim();
            entity.Gstno         = dto.GSTNo?.Trim();
            entity.IsActive      = dto.IsActive;
            await _repo.UpdateAsync(entity);
            return (true, "Vendor updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? (true, "Vendor deactivated.") : (false, "Not found.");
        }
    }
}
