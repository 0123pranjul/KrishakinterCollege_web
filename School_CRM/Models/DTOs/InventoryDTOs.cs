using System.ComponentModel.DataAnnotations;

namespace School_CRM.Models.DTOs
{
    // ============================================================
    // CATEGORY
    // ============================================================
    public class InvCategoryDto
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string CategoryName { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int ProductCount { get; set; }
    }

    // ============================================================
    // UNIT
    // ============================================================
    public class InvUnitDto
    {
        public int UnitId { get; set; }

        [Required, StringLength(50)]
        public string UnitName { get; set; } = null!;

        [Required, StringLength(10)]
        public string UnitShort { get; set; } = null!;

        public bool IsActive { get; set; } = true;
    }

    // ============================================================
    // SUPPLIER
    // ============================================================
    public class InvSupplierDto
    {
        public int SupplierId { get; set; }

        [Required, StringLength(200)]
        public string SupplierName { get; set; } = null!;

        [StringLength(100)]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100), EmailAddress]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? GSTNo { get; set; }

        [Range(0, 9999999.99)]
        public decimal OpeningBalance { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // ============================================================
    // PRODUCT
    // ============================================================
    public class InvProductDto
    {
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }

        [Required, StringLength(200)]
        public string ProductName { get; set; } = null!;

        [Required]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required]
        public int UnitId { get; set; }
        public string? UnitShort { get; set; }

        [Required, Range(0, 9999999.99)]
        public decimal CostPrice { get; set; }

        [Required, Range(0, 9999999.99)]
        public decimal SellingPrice { get; set; }

        [Range(0, 99999)]
        public int ReorderLevel { get; set; } = 5;

        public int? MaxStockLevel { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(20)]
        public string? HSNCode { get; set; }

        [Range(0, 100)]
        public decimal GSTPercent { get; set; }

        public string? ProductImagePath { get; set; }
        public IFormFile? ProductImage { get; set; }

        [StringLength(100)]
        public string? Barcode { get; set; }

        public bool IsActive { get; set; } = true;
        public int CurrentStock { get; set; }

        // For Add form only
        [Range(0, 99999)]
        public int OpeningStock { get; set; }
    }

    public class InvProductSearchDto
    {
        public string? SearchText { get; set; }
        public int? CategoryId { get; set; }
        public string? StockStatus { get; set; } // All / LowStock / OutOfStock / Normal
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    public class InvProductListItemDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string UnitShort { get; set; } = null!;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
        public string StockStatus { get; set; } = null!; // InStock / LowStock / OutOfStock
        public bool IsActive { get; set; }
    }

    // AJAX product lookup for sale/PO forms
    public class InvProductLookupDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int CurrentStock { get; set; }
        public string UnitShort { get; set; } = null!;
        public decimal GSTPercent { get; set; }
    }

    // ============================================================
    // PURCHASE ORDER
    // ============================================================
    public class InvPurchaseOrderDto
    {
        public int POId { get; set; }
        public string? PONumber { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        [Required]
        public DateOnly OrderDate { get; set; }
        public DateOnly? ExpectedDate { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        public string Status { get; set; } = "Draft";
        public decimal TotalAmount { get; set; }

        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<InvPOItemDto> Items { get; set; } = new();
    }

    public class InvPOItemDto
    {
        public int POItemId { get; set; }
        public int POId { get; set; }

        [Required]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? UnitShort { get; set; }

        [Required, Range(1, 99999)]
        public int OrderQty { get; set; }

        public int ReceivedQty { get; set; }

        [Required, Range(0.01, 9999999.99)]
        public decimal UnitCostPrice { get; set; }

        public decimal TotalCost => OrderQty * UnitCostPrice;

        [StringLength(200)]
        public string? Remarks { get; set; }
    }

    // ============================================================
    // STOCK RECEIPT / GRN
    // ============================================================
    public class InvStockReceiptDto
    {
        public int ReceiptId { get; set; }
        public string? GRNNumber { get; set; }

        public int? POId { get; set; }
        public string? PONumber { get; set; }

        [Required]
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }

        [Required]
        public DateOnly ReceiptDate { get; set; }

        [StringLength(100)]
        public string? InvoiceNo { get; set; }

        public DateOnly? InvoiceDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal TotalAmount { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<InvGRNItemDto> Items { get; set; } = new();
    }

    public class InvGRNItemDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int? POItemId { get; set; }
        public int OrderedQty { get; set; }

        [Required, Range(1, 99999)]
        public int ReceivedQty { get; set; }

        [Required, Range(0.01, 9999999.99)]
        public decimal UnitCostPrice { get; set; }

        [StringLength(50)]
        public string? BatchNo { get; set; }

        public DateOnly? ExpiryDate { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }
    }

    // ============================================================
    // SALE / ISSUE BILL
    // ============================================================
    public class InvSaleDto
    {
        public int SaleId { get; set; }
        public string? BillNumber { get; set; }

        [Required]
        public string BillType { get; set; } = "Sale"; // Sale / Issue

        [Required]
        public string CustomerType { get; set; } = null!;

        public int? CustomerId { get; set; }

        [StringLength(200)]
        public string? CustomerName { get; set; }

        [Required]
        public DateOnly SaleDate { get; set; }

        [Required]
        public string PaymentMode { get; set; } = "Cash";

        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public bool IsPaid { get; set; } = true;
        public DateOnly? DueDate { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        [MinLength(1, ErrorMessage = "At least one item is required.")]
        public List<InvSaleItemDto> Items { get; set; } = new();
    }

    public class InvSaleItemDto
    {
        [Required]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? UnitShort { get; set; }
        public int AvailableStock { get; set; }

        [Required, Range(1, 99999)]
        public int Qty { get; set; }

        [Required, Range(0.01, 9999999.99)]
        public decimal UnitSellingPrice { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(0, 100)]
        public decimal GSTPercent { get; set; }

        public decimal LineTotal { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }
    }

    // ============================================================
    // CREDIT PAYMENT
    // ============================================================
    public class InvCreditPaymentDto
    {
        public string CustomerType { get; set; } = null!;
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public decimal TotalDue { get; set; }
        public List<InvUnpaidBillDto> UnpaidBills { get; set; } = new();

        [Required, Range(0.01, 9999999.99)]
        public decimal AmountPaid { get; set; }

        [Required]
        public string PaymentMode { get; set; } = "Cash";

        public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [StringLength(300)]
        public string? Remarks { get; set; }
    }

    public class InvUnpaidBillDto
    {
        public int SaleId { get; set; }
        public string BillNumber { get; set; } = null!;
        public DateOnly SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
    }

    // ============================================================
    // STOCK ADJUSTMENT
    // ============================================================
    public class InvStockAdjustmentDto
    {
        [Required]
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int CurrentStock { get; set; }

        [Required]
        public string AdjustmentType { get; set; } = null!;

        [Required]
        public int AdjustedQty { get; set; } // positive or negative

        public int NewStock => CurrentStock + AdjustedQty;

        [Required, StringLength(300)]
        public string Reason { get; set; } = null!;

        [StringLength(300)]
        public string? Remarks { get; set; }
    }

    // ============================================================
    // DASHBOARD
    // ============================================================
    public class InvAdminDashboardDto
    {
        public decimal TodaySalesTotal { get; set; }
        public int TodayItemsSold { get; set; }
        public int TodayIssues { get; set; }
        public int PendingCreditsCount { get; set; }
        public decimal PendingCreditsAmount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int PendingPOCount { get; set; }

        public List<InvLowStockAlertDto> LowStockItems { get; set; } = new();
        public List<InvLowStockAlertDto> OutOfStockItems { get; set; } = new();
        public List<InvPendingPODto> PendingPOs { get; set; } = new();
        public List<InvCategoryStockValueDto> CategoryStockValues { get; set; } = new();
        public List<InvMonthlySalesDto> MonthlySales { get; set; } = new();
        public List<InvRecentBillDto> RecentBills { get; set; } = new();
        public List<InvTopDebtorDto> TopDebtors { get; set; } = new();
    }

    public class InvMemberDashboardDto
    {
        public decimal PendingDues { get; set; }
        public List<InvUnpaidBillDto> UnpaidBills { get; set; } = new();
        public List<InvRecentBillDto> RecentPurchases { get; set; } = new();
        public List<InvItemPurchaseHistoryDto> ItemHistory { get; set; } = new();
    }

    public class InvLowStockAlertDto
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public int CurrentStock { get; set; }
        public int ReorderLevel { get; set; }
    }

    public class InvPendingPODto
    {
        public int POId { get; set; }
        public string PONumber { get; set; } = null!;
        public string SupplierName { get; set; } = null!;
        public DateOnly OrderDate { get; set; }
        public DateOnly? ExpectedDate { get; set; }
        public string Status { get; set; } = null!;
        public decimal TotalAmount { get; set; }
    }

    public class InvCategoryStockValueDto
    {
        public string CategoryName { get; set; } = null!;
        public int TotalItems { get; set; }
        public decimal StockValue { get; set; }
    }

    public class InvMonthlySalesDto
    {
        public string Month { get; set; } = null!;
        public decimal Total { get; set; }
    }

    public class InvRecentBillDto
    {
        public int SaleId { get; set; }
        public string BillNumber { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public string CustomerType { get; set; } = null!;
        public DateOnly SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMode { get; set; } = null!;
        public bool IsPaid { get; set; }
    }

    public class InvTopDebtorDto
    {
        public string CustomerName { get; set; } = null!;
        public string CustomerType { get; set; } = null!;
        public decimal TotalDue { get; set; }
        public DateOnly OldestBillDate { get; set; }
    }

    public class InvItemPurchaseHistoryDto
    {
        public string ProductName { get; set; } = null!;
        public int TotalQty { get; set; }
        public DateOnly LastPurchase { get; set; }
    }

    // ============================================================
    // SALE LIST FILTER
    // ============================================================
    public class InvSaleFilterDto
    {
        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string? CustomerType { get; set; }
        public string? PaymentMode { get; set; }
        public string? BillType { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
