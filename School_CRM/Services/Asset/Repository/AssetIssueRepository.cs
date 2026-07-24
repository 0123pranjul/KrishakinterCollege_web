using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset.Repository
{
    public class AssetIssueRepository : IAssetIssueRepository
    {
        private readonly LibmanagementContext _db;
        public AssetIssueRepository(LibmanagementContext db) => _db = db;

        public async Task<AsmIssueTransaction?> GetByIdAsync(int issueId) =>
            await _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .FirstOrDefaultAsync(x => x.IssueId == issueId);

        public async Task<AsmIssueTransaction?> GetOpenIssueByUnitIdAsync(int unitId) =>
            await _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Where(x => x.UnitId == unitId && !x.IsReturned)
                .FirstOrDefaultAsync();

        public async Task<bool> HasOpenIssueAsync(int unitId) =>
            await _db.AsmIssueTransactions
                .AnyAsync(x => x.UnitId == unitId && !x.IsReturned);

        public async Task<List<AsmIssueTransaction>> GetByPersonAsync(string type, int id, bool openOnly = false)
        {
            var q = _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Where(x => x.IssuedToType == type && x.IssuedToId == id);
            if (openOnly) q = q.Where(x => !x.IsReturned);
            return await q.OrderByDescending(x => x.IssueDate).ToListAsync();
        }

        public async Task<List<OverdueAssetDto>> GetOverdueListAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var items = await _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Where(x => !x.IsReturned
                         && x.ExpectedReturnDate.HasValue
                         && x.ExpectedReturnDate.Value < today)
                .OrderBy(x => x.ExpectedReturnDate)
                .ToListAsync();

            var result = new List<OverdueAssetDto>();
            foreach (var item in items)
            {
                var name = await GetPersonNameAsync(item.IssuedToType, item.IssuedToId);
                var days = (today.ToDateTime(TimeOnly.MinValue)
                          - item.ExpectedReturnDate!.Value.ToDateTime(TimeOnly.MinValue)).Days;
                result.Add(new OverdueAssetDto
                {
                    IssueId            = item.IssueId,
                    AssetName          = item.Unit.Asset.AssetName,
                    AssetTag           = item.Unit.AssetTag,
                    IssuedTo           = name,
                    IssuedToType       = item.IssuedToType,
                    IssueDate          = item.IssueDate,
                    ExpectedReturnDate = item.ExpectedReturnDate.Value,
                    DaysOverdue        = days
                });
            }
            return result;
        }

        public async Task<List<RecentIssueDto>> GetRecentIssuedAsync(int count = 10)
        {
            var items = await _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Where(x => !x.IsReturned)
                .OrderByDescending(x => x.IssueDate)
                .Take(count)
                .ToListAsync();

            var result = new List<RecentIssueDto>();
            foreach (var item in items)
            {
                var name = await GetPersonNameAsync(item.IssuedToType, item.IssuedToId);
                result.Add(new RecentIssueDto
                {
                    IssueId            = item.IssueId,
                    AssetName          = item.Unit.Asset.AssetName,
                    AssetTag           = item.Unit.AssetTag,
                    IssuedTo           = name,
                    IssuedToType       = item.IssuedToType,
                    IssueDate          = item.IssueDate,
                    ExpectedReturnDate = item.ExpectedReturnDate
                });
            }
            return result;
        }

        public async Task<int> GetIssuedCountAsync() =>
            await _db.AsmIssueTransactions.CountAsync(x => !x.IsReturned);

        public async Task<int> GetOverdueCountAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _db.AsmIssueTransactions
                .CountAsync(x => !x.IsReturned
                              && x.ExpectedReturnDate.HasValue
                              && x.ExpectedReturnDate.Value < today);
        }

        public async Task<AsmIssueTransaction> CreateAsync(AsmIssueTransaction entity)
        {
            _db.AsmIssueTransactions.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task<AsmIssueTransaction> UpdateAsync(AsmIssueTransaction entity)
        {
            _db.AsmIssueTransactions.Update(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        private async Task<string> GetPersonNameAsync(string type, int id)
        {
            return type switch
            {
                "Teacher" => (await _db.TblTeachers.Where(t => t.TeacherId == id)
                                .Select(t => t.TeacherName).FirstOrDefaultAsync()) ?? $"Teacher #{id}",
                "Staff"   => (await _db.Employees.Where(e => e.Id == id)
                                .Select(e => e.Name).FirstOrDefaultAsync()) ?? $"Staff #{id}",
                "Student" => (await _db.TblStudents.Where(s => s.StudentId == id)
                                .Select(s => s.StudentName).FirstOrDefaultAsync()) ?? $"Student #{id}",
                "Location"=> (await _db.AsmLocations.Where(l => l.LocationId == id)
                                .Select(l => l.LocationName).FirstOrDefaultAsync()) ?? $"Location #{id}",
                _         => $"{type} #{id}"
            };
        }
    }
}
