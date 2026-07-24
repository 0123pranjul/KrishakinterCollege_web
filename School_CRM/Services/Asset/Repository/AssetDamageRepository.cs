using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetDamageReportRepository : IAssetDamageReportRepository
    {
        private readonly LibmanagementContext _db;
        public AssetDamageReportRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmDamageLossReport>> GetByUnitIdAsync(int unitId) =>
            await _db.AsmDamageLossReports
                .Where(x => x.UnitId == unitId)
                .OrderByDescending(x => x.ReportDate)
                .ToListAsync();

        public async Task<AsmDamageLossReport?> GetByIdAsync(int id) =>
            await _db.AsmDamageLossReports
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .FirstOrDefaultAsync(x => x.ReportId == id);

        public async Task<int> GetOpenCountAsync() =>
            await _db.AsmDamageLossReports.CountAsync(x => x.Status == "Open");

        public async Task<AsmDamageLossReport> CreateAsync(AsmDamageLossReport entity)
        {
            _db.AsmDamageLossReports.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmDamageLossReport> UpdateAsync(AsmDamageLossReport entity)
        {
            _db.AsmDamageLossReports.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }

    public class AssetDisposalRepository : IAssetDisposalRepository
    {
        private readonly LibmanagementContext _db;
        public AssetDisposalRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmDisposalLog>> GetByUnitIdAsync(int unitId) =>
            await _db.AsmDisposalLogs
                .Where(x => x.UnitId == unitId)
                .OrderByDescending(x => x.DisposalDate)
                .ToListAsync();

        public async Task<AsmDisposalLog?> GetByIdAsync(int id) =>
            await _db.AsmDisposalLogs
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .FirstOrDefaultAsync(x => x.DisposalId == id);

        public async Task<AsmDisposalLog> CreateAsync(AsmDisposalLog entity)
        {
            _db.AsmDisposalLogs.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }

    public class AssetLocationHistoryRepository : IAssetLocationHistoryRepository
    {
        private readonly LibmanagementContext _db;
        public AssetLocationHistoryRepository(LibmanagementContext db) => _db = db;

        public async Task<List<AsmLocationHistory>> GetByUnitIdAsync(int unitId) =>
            await _db.AsmLocationHistories
                .Include(x => x.FromLocation)
                .Include(x => x.ToLocation)
                .Where(x => x.UnitId == unitId)
                .OrderByDescending(x => x.MoveDate)
                .ToListAsync();

        public async Task<AsmLocationHistory> CreateAsync(AsmLocationHistory entity)
        {
            _db.AsmLocationHistories.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
