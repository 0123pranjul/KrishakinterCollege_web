using School_CRM.Models;
using School_CRM.Models.DTOs;

namespace School_CRM.Services.Interface
{
    public interface IInvCategoryService
    {
        Task<List<InvCategory>> GetAllAsync(bool activeOnly = true);
        Task<InvCategory?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(InvCategoryDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(InvCategoryDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public interface IInvUnitService
    {
        Task<List<InvUnit>> GetAllAsync(bool activeOnly = true);
        Task<InvUnit?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(InvUnitDto dto);
        Task<(bool Success, string Message)> UpdateAsync(InvUnitDto dto);
    }

    public interface IInvSupplierService
    {
        Task<List<InvSupplier>> GetAllAsync(bool activeOnly = true);
        Task<InvSupplier?> GetByIdAsync(int id);
        Task<(bool Success, string Message)> CreateAsync(InvSupplierDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(InvSupplierDto dto);
        Task<(bool Success, string Message)> DeleteAsync(int id);
    }

    public interface IInvProductService
    {
        Task<(List<InvProductListItemDto> Items, int Total)> SearchAsync(InvProductSearchDto filter);
        Task<InvProduct?> GetByIdAsync(int id);
        Task<List<InvProductLookupDto>> SearchLookupAsync(string query);
        Task<(bool Success, string Message, int ProductId)> CreateAsync(InvProductDto dto, int createdBy);
        Task<(bool Success, string Message)> UpdateAsync(InvProductDto dto, int updatedBy);
        Task<List<InvLowStockAlertDto>> GetLowStockAsync();
        Task<List<InvLowStockAlertDto>> GetOutOfStockAsync();
    }

    public interface IInvPurchaseOrderService
    {
        Task<List<InvPurchaseOrder>> GetAllAsync(int? supplierId = null, string? status = null);
        Task<InvPurchaseOrder?> GetByIdAsync(int id);
        Task<List<InvPurchaseOrder>> GetPendingAsync();
        Task<(bool Success, string Message, int POId)> CreateAsync(InvPurchaseOrderDto dto, int createdBy);
        Task<(bool Success, string Message)> SendToSupplierAsync(int id, int userId);
        Task<(bool Success, string Message)> CancelAsync(int id);
    }

    public interface IInvStockReceiptService
    {
        Task<List<InvStockReceipt>> GetAllAsync(int? supplierId = null);
        Task<InvStockReceipt?> GetByIdAsync(int id);
        Task<List<InvPOItemDto>> GetPOItemsAsync(int poId);
        Task<(bool Success, string Message, int ReceiptId)> CreateAsync(InvStockReceiptDto dto, int receivedBy);
    }

    public interface IInvSaleService
    {
        Task<(List<InvSaleTransaction> Items, int Total)> GetAllAsync(InvSaleFilterDto filter);
        Task<InvSaleTransaction?> GetByIdAsync(int id);
        Task<(bool Success, string Message, int SaleId)> CreateAsync(InvSaleDto dto, int soldBy);
        Task<InvCreditPaymentDto?> GetCreditDetailsAsync(string type, int id);
        Task<(bool Success, string Message)> CollectPaymentAsync(InvCreditPaymentDto dto, int receivedBy);
        Task<List<InvSaleTransaction>> GetUnpaidByCustomerAsync(string type, int id);
    }

    public interface IInvStockAdjustmentService
    {
        Task<(bool Success, string Message)> AdjustAsync(InvStockAdjustmentDto dto, int userId);
        Task<List<InvStockAdjustment>> GetByProductAsync(int productId);
    }

    public interface IInvDashboardService
    {
        Task<InvAdminDashboardDto> GetAdminDashboardAsync();
        Task<InvMemberDashboardDto> GetMemberDashboardAsync(string type, int id);
    }

    public interface IInvPersonService
    {
        Task<List<InvPersonLookupDto>> GetPersonListAsync(string type, string? search = null);
        Task<InvPersonLookupDto?> GetPersonAsync(string type, int id);
    }

    public class InvPersonLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
