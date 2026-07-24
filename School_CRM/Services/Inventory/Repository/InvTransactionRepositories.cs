using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory.Repository
{
    // ── Purchase Order ────────────────────────────────────────────────────────
    public class InvPurchaseOrderRepository : IInvPurchaseOrderRepository
    {
        private readonly LibmanagementContext _db;
        public InvPurchaseOrderRepository(LibmanagementContext db) => _db = db;

        public async Task<List<InvPurchaseOrder>> GetAllAsync(int? supplierId = null, string? status = null)
        {
            var q = _db.InvPurchaseOrders.Include(x => x.Supplier).AsQueryable();
            if (supplierId.HasValue) q = q.Where(x => x.SupplierId == supplierId);
            if (!string.IsNullOrEmpty(status)) q = q.Where(x => x.Status == status);
            return await q.OrderByDescending(x => x.OrderDate).ToListAsync();
        }

        public async Task<InvPurchaseOrder?> GetByIdWithItemsAsync(int id) =>
            await _db.InvPurchaseOrders
                .Include(x => x.Supplier)
                .Include(x => x.InvPurchaseOrderItems)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(x => x.Poid == id);

        public async Task<List<InvPurchaseOrder>> GetPendingAsync() =>
            await _db.InvPurchaseOrders
                .Include(x => x.Supplier)
                .Where(x => x.Status == "Sent" || x.Status == "PartialReceived")
                .OrderBy(x => x.ExpectedDate)
                .ToListAsync();

        public async Task<InvPurchaseOrder> CreateAsync(InvPurchaseOrder po, List<InvPurchaseOrderItem> items)
        {
            _db.InvPurchaseOrders.Add(po);
            await _db.SaveChangesAsync();

            foreach (var item in items)
            {
                item.Poid = po.Poid;
                _db.InvPurchaseOrderItems.Add(item);
            }

            po.TotalAmount = items.Sum(i => i.OrderQty * i.UnitCostPrice);
            await _db.SaveChangesAsync();
            return po;
        }

        public async Task<bool> UpdateStatusAsync(int id, string status, int? approvedBy = null)
        {
            var po = await _db.InvPurchaseOrders.FindAsync(id);
            if (po == null) return false;
            po.Status = status;
            if (approvedBy.HasValue) { po.ApprovedBy = approvedBy; po.ApprovedAt = DateTime.Now; }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GeneratePONumberAsync()
        {
            var year = DateTime.Today.Year;
            var prefix = $"PO-{year}-";
            var count = await _db.InvPurchaseOrders.CountAsync(x => x.Ponumber.StartsWith(prefix));
            return $"{prefix}{(count + 1):D4}";
        }
    }

    // ── Stock Receipt / GRN ───────────────────────────────────────────────────
    public class InvStockReceiptRepository : IInvStockReceiptRepository
    {
        private readonly LibmanagementContext _db;
        public InvStockReceiptRepository(LibmanagementContext db) => _db = db;

        public async Task<List<InvStockReceipt>> GetAllAsync(int? supplierId = null) =>
            await _db.InvStockReceipts
                .Include(x => x.Supplier)
                .Include(x => x.Po)
                .Where(x => !supplierId.HasValue || x.SupplierId == supplierId)
                .OrderByDescending(x => x.ReceiptDate)
                .ToListAsync();

        public async Task<InvStockReceipt?> GetByIdWithItemsAsync(int id) =>
            await _db.InvStockReceipts
                .Include(x => x.Supplier)
                .Include(x => x.Po)
                .Include(x => x.InvStockReceiptItems)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(x => x.ReceiptId == id);

        public async Task<InvStockReceipt> CreateAsync(InvStockReceipt receipt, List<InvStockReceiptItem> items)
        {
            _db.InvStockReceipts.Add(receipt);
            await _db.SaveChangesAsync();

            foreach (var item in items)
            {
                item.ReceiptId = receipt.ReceiptId;
                _db.InvStockReceiptItems.Add(item);
            }

            receipt.TotalAmount = items.Sum(i => i.ReceivedQty * i.UnitCostPrice);
            await _db.SaveChangesAsync();
            return receipt;
        }

        public async Task<string> GenerateGRNNumberAsync()
        {
            var year = DateTime.Today.Year;
            var prefix = $"GRN-{year}-";
            var count = await _db.InvStockReceipts.CountAsync(x => x.Grnnumber.StartsWith(prefix));
            return $"{prefix}{(count + 1):D4}";
        }
    }

    // ── Sale ──────────────────────────────────────────────────────────────────
    public class InvSaleRepository : IInvSaleRepository
    {
        private readonly LibmanagementContext _db;
        public InvSaleRepository(LibmanagementContext db) => _db = db;

        public async Task<(List<InvSaleTransaction> Items, int Total)> GetAllAsync(InvSaleFilterDto filter)
        {
            var q = _db.InvSaleTransactions.AsQueryable();

            if (filter.FromDate.HasValue) q = q.Where(x => x.SaleDate >= filter.FromDate);
            if (filter.ToDate.HasValue)   q = q.Where(x => x.SaleDate <= filter.ToDate);
            if (!string.IsNullOrEmpty(filter.CustomerType)) q = q.Where(x => x.CustomerType == filter.CustomerType);
            if (!string.IsNullOrEmpty(filter.PaymentMode))  q = q.Where(x => x.PaymentMode == filter.PaymentMode);
            if (!string.IsNullOrEmpty(filter.BillType))     q = q.Where(x => x.BillType == filter.BillType);

            var total = await q.CountAsync();
            var items = await q
                .OrderByDescending(x => x.SaleDate)
                .ThenByDescending(x => x.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<InvSaleTransaction?> GetByIdWithItemsAsync(int id) =>
            await _db.InvSaleTransactions
                .Include(x => x.InvSaleItems)
                .ThenInclude(i => i.Product)
                .ThenInclude(p => p.Unit)
                .FirstOrDefaultAsync(x => x.SaleId == id);

        public async Task<List<InvSaleTransaction>> GetUnpaidByCustomerAsync(string type, int id) =>
            await _db.InvSaleTransactions
                .Where(x => x.CustomerType == type && x.CustomerId == id && !x.IsPaid)
                .OrderBy(x => x.SaleDate)
                .ToListAsync();

        public async Task<InvSaleTransaction> CreateAsync(InvSaleTransaction sale, List<InvSaleItem> items)
        {
            _db.InvSaleTransactions.Add(sale);
            await _db.SaveChangesAsync();

            foreach (var item in items)
            {
                item.SaleId = sale.SaleId;
                _db.InvSaleItems.Add(item);
            }

            await _db.SaveChangesAsync();
            return sale;
        }

        public async Task<bool> MarkPaidAsync(int saleId)
        {
            var sale = await _db.InvSaleTransactions.FindAsync(saleId);
            if (sale == null) return false;
            sale.IsPaid = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateBillNumberAsync()
        {
            var year = DateTime.Today.Year;
            var prefix = $"BILL-{year}-";
            var count = await _db.InvSaleTransactions.CountAsync(x => x.BillNumber.StartsWith(prefix));
            return $"{prefix}{(count + 1):D4}";
        }

        public async Task<decimal> GetTodaySalesTotalAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _db.InvSaleTransactions
                .Where(x => x.SaleDate == today && x.BillType == "Sale")
                .SumAsync(x => (decimal?)x.TotalAmount) ?? 0;
        }

        public async Task<int> GetTodayItemsSoldAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _db.InvSaleItems
                .Where(x => x.Sale.SaleDate == today && x.Sale.BillType == "Sale")
                .SumAsync(x => (int?)x.Qty) ?? 0;
        }

        public async Task<int> GetTodayIssueCountAsync()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return await _db.InvSaleTransactions
                .CountAsync(x => x.SaleDate == today && x.BillType == "Issue");
        }

        public async Task<List<InvMonthlySalesDto>> GetMonthlySalesAsync(int months)
        {
            var from = DateOnly.FromDateTime(DateTime.Today.AddMonths(-months));
            var data = await _db.InvSaleTransactions
                .Where(x => x.SaleDate >= from && x.BillType == "Sale")
                .GroupBy(x => new { x.SaleDate.Year, x.SaleDate.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(x => x.TotalAmount) })
                .OrderBy(x => x.Year).ThenBy(x => x.Month)
                .ToListAsync();

            return data.Select(x => new InvMonthlySalesDto
            {
                Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                Total = x.Total
            }).ToList();
        }

        public async Task<List<InvRecentBillDto>> GetRecentBillsAsync(int count) =>
            await _db.InvSaleTransactions
                .OrderByDescending(x => x.CreatedAt)
                .Take(count)
                .Select(x => new InvRecentBillDto
                {
                    SaleId       = x.SaleId,
                    BillNumber   = x.BillNumber,
                    CustomerName = x.CustomerName ?? $"{x.CustomerType} #{x.CustomerId}",
                    CustomerType = x.CustomerType,
                    SaleDate     = x.SaleDate,
                    TotalAmount  = x.TotalAmount,
                    PaymentMode  = x.PaymentMode,
                    IsPaid       = x.IsPaid
                })
                .ToListAsync();
    }

    // ── Credit Ledger ─────────────────────────────────────────────────────────
    public class InvCreditLedgerRepository : IInvCreditLedgerRepository
    {
        private readonly LibmanagementContext _db;
        public InvCreditLedgerRepository(LibmanagementContext db) => _db = db;

        public async Task<decimal> GetBalanceAsync(string type, int id) =>
            await _db.InvCreditLedgers
                .Where(x => x.CustomerType == type && x.CustomerId == id)
                .SumAsync(x => x.TransactionType == "Debit" ? x.Amount : -x.Amount);

        public async Task<List<InvCreditLedger>> GetByCustomerAsync(string type, int id) =>
            await _db.InvCreditLedgers
                .Where(x => x.CustomerType == type && x.CustomerId == id)
                .OrderByDescending(x => x.TransactionDate)
                .ToListAsync();

        public async Task<InvCreditLedger> CreateAsync(InvCreditLedger entry)
        {
            _db.InvCreditLedgers.Add(entry);
            await _db.SaveChangesAsync();
            return entry;
        }

        public async Task<int> GetPendingCountAsync() =>
            await _db.InvSaleTransactions.CountAsync(x => !x.IsPaid && x.BillType == "Sale");

        public async Task<decimal> GetTotalPendingAmountAsync() =>
            await _db.InvSaleTransactions
                .Where(x => !x.IsPaid && x.BillType == "Sale")
                .SumAsync(x => (decimal?)x.BalanceDue) ?? 0;

        public async Task<List<InvTopDebtorDto>> GetTopDebtorsAsync(int count)
        {
            var unpaid = await _db.InvSaleTransactions
                .Where(x => !x.IsPaid && x.BillType == "Sale" && x.CustomerId.HasValue)
                .GroupBy(x => new { x.CustomerType, x.CustomerId, x.CustomerName })
                .Select(g => new InvTopDebtorDto
                {
                    CustomerName  = g.Key.CustomerName ?? $"{g.Key.CustomerType} #{g.Key.CustomerId}",
                    CustomerType  = g.Key.CustomerType,
                    TotalDue      = g.Sum(x => x.BalanceDue ?? 0),
                    OldestBillDate = g.Min(x => x.SaleDate)
                })
                .OrderByDescending(x => x.TotalDue)
                .Take(count)
                .ToListAsync();

            return unpaid;
        }
    }

    // ── Stock Adjustment ──────────────────────────────────────────────────────
    public class InvStockAdjustmentRepository : IInvStockAdjustmentRepository
    {
        private readonly LibmanagementContext _db;
        public InvStockAdjustmentRepository(LibmanagementContext db) => _db = db;

        public async Task<List<InvStockAdjustment>> GetByProductAsync(int productId) =>
            await _db.InvStockAdjustments
                .Where(x => x.ProductId == productId)
                .OrderByDescending(x => x.AdjustedAt)
                .ToListAsync();

        public async Task<InvStockAdjustment> CreateAsync(InvStockAdjustment entity)
        {
            _db.InvStockAdjustments.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }
    }
}
