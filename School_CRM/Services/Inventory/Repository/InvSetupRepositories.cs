using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory.Repository
{
    // ── Category ─────────────────────────────────────────────────────────────
    public class InvCategoryRepository : IInvCategoryRepository
    {
        private readonly LibmanagementContext _db;
        public InvCategoryRepository(LibmanagementContext db) => _db = db;

        public async Task<List<InvCategory>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.InvCategories.AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.CategoryName).ToListAsync();
        }

        public async Task<InvCategory?> GetByIdAsync(int id) =>
            await _db.InvCategories.FindAsync(id);

        public async Task<bool> ExistsAsync(string name, int excludeId = 0) =>
            await _db.InvCategories.AnyAsync(x => x.CategoryName == name && x.CategoryId != excludeId);

        public async Task<InvCategory> CreateAsync(InvCategory entity)
        {
            _db.InvCategories.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<InvCategory> UpdateAsync(InvCategory entity)
        {
            _db.InvCategories.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;
            var hasProducts = await _db.InvProducts.AnyAsync(x => x.CategoryId == id && x.IsActive);
            if (hasProducts) return false;
            entity.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }
    }

    // ── Unit ──────────────────────────────────────────────────────────────────
    public class InvUnitRepository : IInvUnitRepository
    {
        private readonly LibmanagementContext _db;
        public InvUnitRepository(LibmanagementContext db) => _db = db;

        public async Task<List<InvUnit>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.InvUnits.AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.UnitName).ToListAsync();
        }

        public async Task<InvUnit?> GetByIdAsync(int id) =>
            await _db.InvUnits.FindAsync(id);

        public async Task<InvUnit> CreateAsync(InvUnit entity)
        {
            _db.InvUnits.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<InvUnit> UpdateAsync(InvUnit entity)
        {
            _db.InvUnits.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }

    // ── Supplier ──────────────────────────────────────────────────────────────
    public class InvSupplierRepository : IInvSupplierRepository
    {
        private readonly LibmanagementContext _db;
        public InvSupplierRepository(LibmanagementContext db) => _db = db;

        public async Task<List<InvSupplier>> GetAllAsync(bool activeOnly = true)
        {
            var q = _db.InvSuppliers.AsQueryable();
            if (activeOnly) q = q.Where(x => x.IsActive);
            return await q.OrderBy(x => x.SupplierName).ToListAsync();
        }

        public async Task<InvSupplier?> GetByIdAsync(int id) =>
            await _db.InvSuppliers.FindAsync(id);

        public async Task<InvSupplier> CreateAsync(InvSupplier entity)
        {
            _db.InvSuppliers.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<InvSupplier> UpdateAsync(InvSupplier entity)
        {
            _db.InvSuppliers.Update(entity);
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
