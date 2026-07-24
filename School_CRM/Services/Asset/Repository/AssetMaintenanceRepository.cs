using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetMaintenanceRepository : IAssetMaintenanceRepository
    {
        private readonly LibmanagementContext _db;
        public AssetMaintenanceRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmMaintenanceLog>> GetByUnitIdAsync(int unitId) =>
            await _db.AsmMaintenanceLogs
                .Include(x => x.Vendor)
                .Where(x => x.UnitId == unitId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();

        public async Task<AsmMaintenanceLog?> GetByIdAsync(int id) =>
            await _db.AsmMaintenanceLogs
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Include(x => x.Vendor)
                .FirstOrDefaultAsync(x => x.MaintenanceId == id);

        public async Task<List<MaintenanceAlertDto>> GetPendingAsync() =>
            await _db.AsmMaintenanceLogs
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Where(x => x.Status == "Pending" || x.Status == "InProgress")
                .OrderBy(x => x.StartDate)
                .Select(x => new MaintenanceAlertDto
                {
                    MaintenanceId   = x.MaintenanceId,
                    AssetName       = x.Unit.Asset.AssetName,
                    AssetTag        = x.Unit.AssetTag,
                    MaintenanceType = x.MaintenanceType,
                    StartDate       = x.StartDate,
                    Status          = x.Status
                })
                .ToListAsync();

        public async Task<int> GetPendingCountAsync() =>
            await _db.AsmMaintenanceLogs
                .CountAsync(x => x.Status == "Pending" || x.Status == "InProgress");

        public async Task<AsmMaintenanceLog> CreateAsync(AsmMaintenanceLog entity)
        {
            _db.AsmMaintenanceLogs.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmMaintenanceLog> UpdateAsync(AsmMaintenanceLog entity)
        {
            _db.AsmMaintenanceLogs.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
