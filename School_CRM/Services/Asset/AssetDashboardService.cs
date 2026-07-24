using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Asset
{
    public class AssetDashboardService : IAssetDashboardService
    {
        private readonly LibmanagementContext     _db;
        private readonly IAssetIssueRepository   _issueRepo;
        private readonly IAssetMaintenanceRepository _maintRepo;
        private readonly IAssetDamageReportRepository _damageRepo;

        public AssetDashboardService(
            LibmanagementContext db,
            IAssetIssueRepository issueRepo,
            IAssetMaintenanceRepository maintRepo,
            IAssetDamageReportRepository damageRepo)
        {
            _db         = db;
            _issueRepo  = issueRepo;
            _maintRepo  = maintRepo;
            _damageRepo = damageRepo;
        }

        public async Task<AssetAdminDashboardDto> GetAdminDashboardAsync()
        {
            var today       = DateOnly.FromDateTime(DateTime.Today);
            var warningDate = today.AddDays(30);

            var totalAssets = await _db.AsmAssets.Where(x => x.IsActive).SumAsync(x => (int?)x.TotalUnits) ?? 0;
            var available   = await _db.AsmAssets.Where(x => x.IsActive).SumAsync(x => (int?)x.AvailableUnits) ?? 0;
            var issued      = await _issueRepo.GetIssuedCountAsync();
            var underRepair = await _db.AsmAssetUnits.CountAsync(x => x.UnitCondition == "UnderRepair" && x.IsActive);
            var overdue     = await _issueRepo.GetOverdueCountAsync();
            var warrantyExp = await _db.AsmAssetUnits
                .CountAsync(x => x.IsActive && x.WarrantyExpiry.HasValue
                              && x.WarrantyExpiry.Value >= today
                              && x.WarrantyExpiry.Value <= warningDate);
            var pendingMaint = await _maintRepo.GetPendingCountAsync();
            var openDamage   = await _damageRepo.GetOpenCountAsync();

            // Category-wise stock
            var catStock = await _db.AsmAssets
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new CategoryStockDto
                {
                    CategoryName = g.Key,
                    Total        = g.Sum(x => x.TotalUnits),
                    Available    = g.Sum(x => x.AvailableUnits),
                    Issued       = g.Sum(x => x.TotalUnits - x.AvailableUnits)
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            // Location-wise stock
            var locStock = await _db.AsmAssetUnits
                .Include(x => x.CurrentLocation)
                .Where(x => x.IsActive && x.CurrentLocationId.HasValue)
                .GroupBy(x => x.CurrentLocation!.LocationName)
                .Select(g => new LocationStockDto
                {
                    LocationName = g.Key,
                    TotalAssets  = g.Count(),
                    IssuedCount  = g.Count(u => !u.IsAvailable)
                })
                .OrderByDescending(x => x.TotalAssets)
                .ToListAsync();

            // Warranty alerts
            var warrantyAlerts = await _db.AsmAssetUnits
                .Include(x => x.Asset)
                .Include(x => x.CurrentLocation)
                .Where(x => x.IsActive && x.WarrantyExpiry.HasValue
                         && x.WarrantyExpiry.Value >= today
                         && x.WarrantyExpiry.Value <= warningDate)
                .Select(x => new WarrantyAlertDto
                {
                    UnitId        = x.UnitId,
                    AssetName     = x.Asset.AssetName,
                    AssetTag      = x.AssetTag,
                    LocationName  = x.CurrentLocation != null ? x.CurrentLocation.LocationName : null,
                    WarrantyExpiry = x.WarrantyExpiry!.Value,
                    DaysLeft      = 0 // calculated below
                })
                .ToListAsync();

            foreach (var w in warrantyAlerts)
                w.DaysLeft = (w.WarrantyExpiry.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;

            return new AssetAdminDashboardDto
            {
                TotalAssets          = totalAssets,
                AvailableUnits       = available,
                IssuedUnits          = issued,
                UnderRepair          = underRepair,
                OverdueReturns       = overdue,
                WarrantyExpiringSoon = warrantyExp,
                PendingMaintenance   = pendingMaint,
                OpenDamageReports    = openDamage,
                CategoryStock        = catStock,
                OverdueAssets        = await _issueRepo.GetOverdueListAsync(),
                WarrantyAlerts       = warrantyAlerts,
                PendingMaintenanceList = await _maintRepo.GetPendingAsync(),
                RecentIssues         = await _issueRepo.GetRecentIssuedAsync(10),
                LocationStock        = locStock
            };
        }

        public async Task<AssetMemberDashboardDto> GetMemberDashboardAsync(string userType, int userId)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var openIssues = await _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .ThenInclude(a => a.Category)
                .Where(x => x.IssuedToType == userType && x.IssuedToId == userId && !x.IsReturned)
                .OrderBy(x => x.ExpectedReturnDate)
                .ToListAsync();

            var myIssued = openIssues.Select(x =>
            {
                int? daysLeft = x.ExpectedReturnDate.HasValue
                    ? (x.ExpectedReturnDate.Value.ToDateTime(TimeOnly.MinValue)
                       - today.ToDateTime(TimeOnly.MinValue)).Days
                    : (int?)null;

                return new MyIssuedAssetDto
                {
                    IssueId            = x.IssueId,
                    AssetName          = x.Unit.Asset.AssetName,
                    AssetTag           = x.Unit.AssetTag,
                    CategoryName       = x.Unit.Asset.Category.CategoryName,
                    IssueDate          = x.IssueDate,
                    ExpectedReturnDate = x.ExpectedReturnDate,
                    DaysRemaining      = daysLeft,
                    IsOverdue          = daysLeft.HasValue && daysLeft.Value < 0,
                    Status             = daysLeft.HasValue && daysLeft.Value < 0 ? "Overdue"
                                       : daysLeft.HasValue && daysLeft.Value <= 3 ? "Due Soon"
                                       : "Active"
                };
            }).ToList();

            var history = await _db.AsmIssueTransactions
                .Include(x => x.Unit).ThenInclude(u => u.Asset)
                .Where(x => x.IssuedToType == userType && x.IssuedToId == userId && x.IsReturned)
                .OrderByDescending(x => x.ReturnDate)
                .Take(10)
                .Select(x => new MyIssueHistoryDto
                {
                    IssueId           = x.IssueId,
                    AssetName         = x.Unit.Asset.AssetName,
                    AssetTag          = x.Unit.AssetTag,
                    IssueDate         = x.IssueDate,
                    ReturnDate        = x.ReturnDate,
                    ConditionOnReturn = x.ConditionOnReturn,
                    DamageFine        = x.DamageFine,
                    TransactionStatus = x.TransactionStatus
                })
                .ToListAsync();

            return new AssetMemberDashboardDto
            {
                MyIssuedAssets = myIssued,
                MyIssueHistory = history
            };
        }
    }

    public class AssetPersonService : IAssetPersonService
    {
        private readonly LibmanagementContext _db;
        public AssetPersonService(LibmanagementContext db) => _db = db;

        public async Task<List<PersonLookupDto>> GetPersonListAsync(string type, string? search = null)
        {
            return type switch
            {
                "Teacher" => await GetTeachersAsync(search),
                "Staff"   => await GetStaffAsync(search),
                "Student" => await GetStudentsAsync(search),
                "Location"=> await GetLocationsAsync(search),
                _         => new List<PersonLookupDto>()
            };
        }

        public async Task<PersonLookupDto?> GetPersonAsync(string type, int id)
        {
            return type switch
            {
                "Teacher" => await _db.TblTeachers.Where(t => t.TeacherId == id)
                    .Select(t => new PersonLookupDto { Id = t.TeacherId, Name = t.TeacherName, Code = t.Email ?? "", Type = "Teacher" })
                    .FirstOrDefaultAsync(),
                "Staff" => await _db.Employees.Where(e => e.Id == id)
                    .Select(e => new PersonLookupDto { Id = e.Id, Name = e.Name ?? "", Code = e.EmployeeCode ?? "", Type = "Staff" })
                    .FirstOrDefaultAsync(),
                "Student" => await _db.TblStudents.Where(s => s.StudentId == id)
                    .Select(s => new PersonLookupDto { Id = s.StudentId, Name = s.StudentName ?? "", Code = s.AdmissionNo ?? "", Type = "Student" })
                    .FirstOrDefaultAsync(),
                "Location" => await _db.AsmLocations.Where(l => l.LocationId == id)
                    .Select(l => new PersonLookupDto { Id = l.LocationId, Name = l.LocationName, Code = l.LocationType, Type = "Location" })
                    .FirstOrDefaultAsync(),
                _ => null
            };
        }

        private async Task<List<PersonLookupDto>> GetTeachersAsync(string? search)
        {
            var q = _db.TblTeachers.Where(t => t.IsActive == true);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t => t.TeacherName.Contains(search));
            return await q.Take(50).Select(t => new PersonLookupDto
                { Id = t.TeacherId, Name = t.TeacherName, Code = t.Email ?? "", Type = "Teacher" }).ToListAsync();
        }

        private async Task<List<PersonLookupDto>> GetStaffAsync(string? search)
        {
            var q = _db.Employees.Where(e => e.IsActive == true);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(e => e.Name != null && e.Name.Contains(search));
            return await q.Take(50).Select(e => new PersonLookupDto
                { Id = e.Id, Name = e.Name ?? "", Code = e.EmployeeCode ?? "", Type = "Staff" }).ToListAsync();
        }

        private async Task<List<PersonLookupDto>> GetStudentsAsync(string? search)
        {
            var q = _db.TblStudents.Where(s => s.IsActive == true);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.StudentName != null && s.StudentName.Contains(search));
            return await q.Take(50).Select(s => new PersonLookupDto
                { Id = s.StudentId, Name = s.StudentName ?? "", Code = s.AdmissionNo ?? "", Type = "Student" }).ToListAsync();
        }

        private async Task<List<PersonLookupDto>> GetLocationsAsync(string? search)
        {
            var q = _db.AsmLocations.Where(l => l.IsActive);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(l => l.LocationName.Contains(search));
            return await q.Take(50).Select(l => new PersonLookupDto
                { Id = l.LocationId, Name = l.LocationName, Code = l.LocationType, Type = "Location" }).ToListAsync();
        }
    }
}
