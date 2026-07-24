using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory
{
    public class InvDashboardService : IInvDashboardService
    {
        private readonly IInvSaleRepository         _saleRepo;
        private readonly IInvCreditLedgerRepository _creditRepo;
        private readonly IInvProductRepository      _productRepo;
        private readonly IInvPurchaseOrderRepository _poRepo;
        private readonly LibmanagementContext        _db;

        public InvDashboardService(
            IInvSaleRepository saleRepo,
            IInvCreditLedgerRepository creditRepo,
            IInvProductRepository productRepo,
            IInvPurchaseOrderRepository poRepo,
            LibmanagementContext db)
        {
            _saleRepo    = saleRepo;
            _creditRepo  = creditRepo;
            _productRepo = productRepo;
            _poRepo      = poRepo;
            _db          = db;
        }

        public async Task<InvAdminDashboardDto> GetAdminDashboardAsync()
        {
            var todaySales   = await _saleRepo.GetTodaySalesTotalAsync();
            var todayItems   = await _saleRepo.GetTodayItemsSoldAsync();
            var todayIssues  = await _saleRepo.GetTodayIssueCountAsync();
            var pendingCount = await _creditRepo.GetPendingCountAsync();
            var pendingAmt   = await _creditRepo.GetTotalPendingAmountAsync();
            var lowStock     = await _productRepo.GetLowStockAsync();
            var outOfStock   = await _productRepo.GetOutOfStockAsync();
            var pendingPOs   = await _poRepo.GetPendingAsync();
            var monthlySales = await _saleRepo.GetMonthlySalesAsync(6);
            var recentBills  = await _saleRepo.GetRecentBillsAsync(10);
            var topDebtors   = await _creditRepo.GetTopDebtorsAsync(5);

            // Category-wise stock value
            var catStock = await _db.InvProducts
                .Include(x => x.Category)
                .Where(x => x.IsActive)
                .GroupBy(x => x.Category.CategoryName)
                .Select(g => new InvCategoryStockValueDto
                {
                    CategoryName = g.Key,
                    TotalItems   = g.Count(),
                    StockValue   = g.Sum(x => x.CurrentStock * x.CostPrice)
                })
                .OrderByDescending(x => x.StockValue)
                .ToListAsync();

            return new InvAdminDashboardDto
            {
                TodaySalesTotal      = todaySales,
                TodayItemsSold       = todayItems,
                TodayIssues          = todayIssues,
                PendingCreditsCount  = pendingCount,
                PendingCreditsAmount = pendingAmt,
                LowStockCount        = lowStock.Count,
                OutOfStockCount      = outOfStock.Count,
                PendingPOCount       = pendingPOs.Count,
                LowStockItems        = lowStock,
                OutOfStockItems      = outOfStock,
                PendingPOs           = pendingPOs.Select(p => new InvPendingPODto
                {
                    POId         = p.Poid,
                    PONumber     = p.Ponumber,
                    SupplierName = p.Supplier.SupplierName,
                    OrderDate    = p.OrderDate,
                    ExpectedDate = p.ExpectedDate,
                    Status       = p.Status,
                    TotalAmount  = p.TotalAmount
                }).ToList(),
                CategoryStockValues = catStock,
                MonthlySales        = monthlySales,
                RecentBills         = recentBills,
                TopDebtors          = topDebtors
            };
        }

        public async Task<InvMemberDashboardDto> GetMemberDashboardAsync(string type, int id)
        {
            var balance  = await _creditRepo.GetBalanceAsync(type, id);
            var unpaid   = await _db.InvSaleTransactions
                .Where(x => x.CustomerType == type && x.CustomerId == id && !x.IsPaid)
                .Select(x => new InvUnpaidBillDto
                {
                    SaleId      = x.SaleId,
                    BillNumber  = x.BillNumber,
                    SaleDate    = x.SaleDate,
                    TotalAmount = x.TotalAmount,
                    AmountPaid  = x.AmountPaid,
                    BalanceDue  = x.BalanceDue ?? 0
                })
                .ToListAsync();

            var recent = await _db.InvSaleTransactions
                .Where(x => x.CustomerType == type && x.CustomerId == id)
                .OrderByDescending(x => x.SaleDate)
                .Take(10)
                .Select(x => new InvRecentBillDto
                {
                    SaleId       = x.SaleId,
                    BillNumber   = x.BillNumber,
                    CustomerName = x.CustomerName ?? "",
                    CustomerType = x.CustomerType,
                    SaleDate     = x.SaleDate,
                    TotalAmount  = x.TotalAmount,
                    PaymentMode  = x.PaymentMode,
                    IsPaid       = x.IsPaid
                })
                .ToListAsync();

            var itemHistory = await _db.InvSaleItems
                .Include(x => x.Sale)
                .Include(x => x.Product)
                .Where(x => x.Sale.CustomerType == type && x.Sale.CustomerId == id)
                .GroupBy(x => x.Product.ProductName)
                .Select(g => new InvItemPurchaseHistoryDto
                {
                    ProductName  = g.Key,
                    TotalQty     = g.Sum(x => x.Qty),
                    LastPurchase = g.Max(x => x.Sale.SaleDate)
                })
                .OrderByDescending(x => x.LastPurchase)
                .ToListAsync();

            return new InvMemberDashboardDto
            {
                PendingDues    = balance,
                UnpaidBills    = unpaid,
                RecentPurchases = recent,
                ItemHistory    = itemHistory
            };
        }
    }

    public class InvPersonService : IInvPersonService
    {
        private readonly LibmanagementContext _db;
        public InvPersonService(LibmanagementContext db) => _db = db;

        public async Task<List<InvPersonLookupDto>> GetPersonListAsync(string type, string? search = null)
        {
            return type switch
            {
                "Student" => await GetStudentsAsync(search),
                "Teacher" => await GetTeachersAsync(search),
                "Staff"   => await GetStaffAsync(search),
                _         => new List<InvPersonLookupDto>()
            };
        }

        public async Task<InvPersonLookupDto?> GetPersonAsync(string type, int id)
        {
            return type switch
            {
                "Student" => await _db.TblStudents.Where(s => s.StudentId == id)
                    .Select(s => new InvPersonLookupDto { Id = s.StudentId, Name = s.StudentName ?? "", Code = s.AdmissionNo ?? "", Type = "Student" })
                    .FirstOrDefaultAsync(),
                "Teacher" => await _db.TblTeachers.Where(t => t.TeacherId == id)
                    .Select(t => new InvPersonLookupDto { Id = t.TeacherId, Name = t.TeacherName, Code = t.Email ?? "", Type = "Teacher" })
                    .FirstOrDefaultAsync(),
                "Staff" => await _db.Employees.Where(e => e.Id == id)
                    .Select(e => new InvPersonLookupDto { Id = e.Id, Name = e.Name ?? "", Code = e.EmployeeCode ?? "", Type = "Staff" })
                    .FirstOrDefaultAsync(),
                _ => null
            };
        }

        private async Task<List<InvPersonLookupDto>> GetStudentsAsync(string? search)
        {
            var q = _db.TblStudents.Where(s => s.IsActive == true);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(s => s.StudentName != null && s.StudentName.Contains(search));
            return await q.Take(50).Select(s => new InvPersonLookupDto
                { Id = s.StudentId, Name = s.StudentName ?? "", Code = s.AdmissionNo ?? "", Type = "Student" }).ToListAsync();
        }

        private async Task<List<InvPersonLookupDto>> GetTeachersAsync(string? search)
        {
            var q = _db.TblTeachers.Where(t => t.IsActive == true);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(t => t.TeacherName.Contains(search));
            return await q.Take(50).Select(t => new InvPersonLookupDto
                { Id = t.TeacherId, Name = t.TeacherName, Code = t.Email ?? "", Type = "Teacher" }).ToListAsync();
        }

        private async Task<List<InvPersonLookupDto>> GetStaffAsync(string? search)
        {
            var q = _db.Employees.Where(e => e.IsActive == true);
            if (!string.IsNullOrWhiteSpace(search))
                q = q.Where(e => e.Name != null && e.Name.Contains(search));
            return await q.Take(50).Select(e => new InvPersonLookupDto
                { Id = e.Id, Name = e.Name ?? "", Code = e.EmployeeCode ?? "", Type = "Staff" }).ToListAsync();
        }
    }
}
