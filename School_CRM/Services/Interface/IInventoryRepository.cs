using School_CRM.Models;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Interface
{
    public interface IInvCategoryRepository
    {
        Task<List<InvCategory>> GetAllAsync(bool activeOnly = true);
        Task<InvCategory?> GetByIdAsync(int id);
        Task<bool> ExistsAsync(string name, int excludeId = 0);
        Task<InvCategory> CreateAsync(InvCategory entity);
        Task<InvCategory> UpdateAsync(InvCategory entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IInvUnitRepository
    {
        Task<List<InvUnit>> GetAllAsync(bool activeOnly = true);
        Task<InvUnit?> GetByIdAsync(int id);
        Task<InvUnit> CreateAsync(InvUnit entity);
        Task<InvUnit> UpdateAsync(InvUnit entity);
    }

    public interface IInvSupplierRepository
    {
        Task<List<InvSupplier>> GetAllAsync(bool activeOnly = true);
        Task<InvSupplier?> GetByIdAsync(int id);
        Task<InvSupplier> CreateAsync(InvSupplier entity);
        Task<InvSupplier> UpdateAsync(InvSupplier entity);
        Task<bool> DeleteAsync(int id);
    }

    public interface IInvProductRepository
    {
        Task<(List<InvProductListItemDto> Items, int Total)> SearchAsync(InvProductSearchDto filter);
        Task<InvProduct?> GetByIdAsync(int id);
        Task<List<InvProductLookupDto>> SearchLookupAsync(string query);
        Task<InvProduct> CreateAsync(InvProduct entity);
        Task<InvProduct> UpdateAsync(InvProduct entity);
        Task<bool> UpdateStockAsync(int productId, int delta);
        Task<string> GenerateProductCodeAsync(int categoryId, string catCode);
        Task<List<InvLowStockAlertDto>> GetLowStockAsync();
        Task<List<InvLowStockAlertDto>> GetOutOfStockAsync();
    }

    public interface IInvPurchaseOrderRepository
    {
        Task<List<InvPurchaseOrder>> GetAllAsync(int? supplierId = null, string? status = null);
        Task<InvPurchaseOrder?> GetByIdWithItemsAsync(int id);
        Task<List<InvPurchaseOrder>> GetPendingAsync();
        Task<InvPurchaseOrder> CreateAsync(InvPurchaseOrder po, List<InvPurchaseOrderItem> items);
        Task<bool> UpdateStatusAsync(int id, string status, int? approvedBy = null);
        Task<string> GeneratePONumberAsync();
    }

    public interface IInvStockReceiptRepository
    {
        Task<List<InvStockReceipt>> GetAllAsync(int? supplierId = null);
        Task<InvStockReceipt?> GetByIdWithItemsAsync(int id);
        Task<InvStockReceipt> CreateAsync(InvStockReceipt receipt, List<InvStockReceiptItem> items);
        Task<string> GenerateGRNNumberAsync();
    }

    public interface IInvSaleRepository
    {
        Task<(List<InvSaleTransaction> Items, int Total)> GetAllAsync(InvSaleFilterDto filter);
        Task<InvSaleTransaction?> GetByIdWithItemsAsync(int id);
        Task<List<InvSaleTransaction>> GetUnpaidByCustomerAsync(string type, int id);
        Task<InvSaleTransaction> CreateAsync(InvSaleTransaction sale, List<InvSaleItem> items);
        Task<bool> MarkPaidAsync(int saleId);
        Task<string> GenerateBillNumberAsync();
        Task<decimal> GetTodaySalesTotalAsync();
        Task<int> GetTodayItemsSoldAsync();
        Task<int> GetTodayIssueCountAsync();
        Task<List<InvMonthlySalesDto>> GetMonthlySalesAsync(int months);
        Task<List<InvRecentBillDto>> GetRecentBillsAsync(int count);
    }

    public interface IInvCreditLedgerRepository
    {
        Task<decimal> GetBalanceAsync(string type, int id);
        Task<List<InvCreditLedger>> GetByCustomerAsync(string type, int id);
        Task<InvCreditLedger> CreateAsync(InvCreditLedger entry);
        Task<int> GetPendingCountAsync();
        Task<decimal> GetTotalPendingAmountAsync();
        Task<List<InvTopDebtorDto>> GetTopDebtorsAsync(int count);
    }

    public interface IInvStockAdjustmentRepository
    {
        Task<List<InvStockAdjustment>> GetByProductAsync(int productId);
        Task<InvStockAdjustment> CreateAsync(InvStockAdjustment entity);
    }
}
