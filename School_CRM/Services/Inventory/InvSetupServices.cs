using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory
{
    public class InvCategoryService : IInvCategoryService
    {
        private readonly IInvCategoryRepository _repo;
        public InvCategoryService(IInvCategoryRepository repo) => _repo = repo;

        public Task<List<InvCategory>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<InvCategory?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(InvCategoryDto dto, int createdBy)
        {
            if (await _repo.ExistsAsync(dto.CategoryName))
                return (false, $"Category '{dto.CategoryName}' already exists.");

            await _repo.CreateAsync(new InvCategory
            {
                CategoryName = dto.CategoryName.Trim(),
                Description  = dto.Description?.Trim(),
                IsActive     = dto.IsActive,
                CreatedAt    = DateTime.Now,
                CreatedBy    = createdBy
            });
            return (true, "Category created.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(InvCategoryDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.CategoryId);
            if (entity == null) return (false, "Not found.");
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
            return ok ? (true, "Category deactivated.") : (false, "Cannot delete — has active products.");
        }
    }

    public class InvUnitService : IInvUnitService
    {
        private readonly IInvUnitRepository _repo;
        public InvUnitService(IInvUnitRepository repo) => _repo = repo;

        public Task<List<InvUnit>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<InvUnit?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(InvUnitDto dto)
        {
            await _repo.CreateAsync(new InvUnit
            {
                UnitName  = dto.UnitName.Trim(),
                UnitShort = dto.UnitShort.Trim(),
                IsActive  = dto.IsActive
            });
            return (true, "Unit created.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(InvUnitDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.UnitId);
            if (entity == null) return (false, "Not found.");
            entity.UnitName  = dto.UnitName.Trim();
            entity.UnitShort = dto.UnitShort.Trim();
            entity.IsActive  = dto.IsActive;
            await _repo.UpdateAsync(entity);
            return (true, "Unit updated.");
        }
    }

    public class InvSupplierService : IInvSupplierService
    {
        private readonly IInvSupplierRepository _repo;
        public InvSupplierService(IInvSupplierRepository repo) => _repo = repo;

        public Task<List<InvSupplier>> GetAllAsync(bool activeOnly = true) => _repo.GetAllAsync(activeOnly);
        public Task<InvSupplier?> GetByIdAsync(int id) => _repo.GetByIdAsync(id);

        public async Task<(bool Success, string Message)> CreateAsync(InvSupplierDto dto, int createdBy)
        {
            await _repo.CreateAsync(new InvSupplier
            {
                SupplierName  = dto.SupplierName.Trim(),
                ContactPerson = dto.ContactPerson?.Trim(),
                Phone         = dto.Phone?.Trim(),
                Email         = dto.Email?.Trim(),
                Address       = dto.Address?.Trim(),
                Gstno         = dto.GSTNo?.Trim(),
                OpeningBalance = dto.OpeningBalance,
                IsActive      = dto.IsActive,
                CreatedAt     = DateTime.Now,
                CreatedBy     = createdBy
            });
            return (true, "Supplier created.");
        }

        public async Task<(bool Success, string Message)> UpdateAsync(InvSupplierDto dto)
        {
            var entity = await _repo.GetByIdAsync(dto.SupplierId);
            if (entity == null) return (false, "Not found.");
            entity.SupplierName  = dto.SupplierName.Trim();
            entity.ContactPerson = dto.ContactPerson?.Trim();
            entity.Phone         = dto.Phone?.Trim();
            entity.Email         = dto.Email?.Trim();
            entity.Address       = dto.Address?.Trim();
            entity.Gstno         = dto.GSTNo?.Trim();
            entity.OpeningBalance = dto.OpeningBalance;
            entity.IsActive      = dto.IsActive;
            await _repo.UpdateAsync(entity);
            return (true, "Supplier updated.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            var ok = await _repo.DeleteAsync(id);
            return ok ? (true, "Supplier deactivated.") : (false, "Not found.");
        }
    }
}
