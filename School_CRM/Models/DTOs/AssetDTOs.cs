using System.ComponentModel.DataAnnotations;

namespace School_CRM.Models.DTOs
{
    // ============================================================
    // CATEGORY
    // ============================================================
    public class AssetCategoryDto
    {
        public int CategoryId { get; set; }

        [Required, StringLength(100)]
        public string CategoryName { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
        public int TotalAssets { get; set; }
    }

    // ============================================================
    // SUB-CATEGORY
    // ============================================================
    public class AssetSubCategoryDto
    {
        public int SubCategoryId { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required, StringLength(100)]
        public string SubCategoryName { get; set; } = null!;

        [StringLength(255)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }

    // ============================================================
    // LOCATION
    // ============================================================
    public class AssetLocationDto
    {
        public int LocationId { get; set; }

        [Required, StringLength(150)]
        public string LocationName { get; set; } = null!;

        [Required]
        public string LocationType { get; set; } = "Classroom";

        [StringLength(20)]
        public string? Floor { get; set; }

        [StringLength(50)]
        public string? Building { get; set; }

        public bool IsActive { get; set; } = true;
        public int TotalAssets { get; set; }
    }

    // ============================================================
    // VENDOR
    // ============================================================
    public class AssetVendorDto
    {
        public int VendorId { get; set; }

        [Required, StringLength(200)]
        public string VendorName { get; set; } = null!;

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

        public bool IsActive { get; set; } = true;
    }

    // ============================================================
    // ASSET MASTER
    // ============================================================
    public class AssetMasterDto
    {
        public int AssetId { get; set; }

        [Required, StringLength(200)]
        public string AssetName { get; set; } = null!;

        public string? AssetCode { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(100)]
        public string? Model { get; set; }

        [StringLength(1000)]
        public string? Specifications { get; set; }

        [Required, Range(0.01, 9999999.99)]
        public decimal UnitPrice { get; set; }

        public int TotalUnits { get; set; }
        public int AvailableUnits { get; set; }
        public bool IsIssuable { get; set; } = true;
        public string? AssetImagePath { get; set; }
        public IFormFile? AssetImage { get; set; }
        public bool IsActive { get; set; } = true;

        // For Add form — how many physical units to create
        [Range(1, 500)]
        public int NumberOfUnits { get; set; } = 1;

        // Per-unit purchase details (applied to all new units)
        public DateOnly? PurchaseDate { get; set; }
        public int? VendorId { get; set; }

        [StringLength(100)]
        public string? InvoiceNo { get; set; }

        public DateOnly? WarrantyExpiry { get; set; }
        public DateOnly? AMCExpiry { get; set; }
        public int? DefaultLocationId { get; set; }
        public decimal PurchasePrice { get; set; }
    }

    public class AssetListItemDto
    {
        public int AssetId { get; set; }
        public string AssetCode { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string? SubCategoryName { get; set; }
        public string? Brand { get; set; }
        public int TotalUnits { get; set; }
        public int AvailableUnits { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsIssuable { get; set; }
        public bool IsActive { get; set; }
    }

    public class AssetSearchDto
    {
        public string? SearchText { get; set; }
        public int? CategoryId { get; set; }
        public int? SubCategoryId { get; set; }
        public bool? OnlyAvailable { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    // ============================================================
    // ASSET UNIT
    // ============================================================
    public class AssetUnitDto
    {
        public int UnitId { get; set; }
        public int AssetId { get; set; }
        public string AssetTag { get; set; } = null!;
        public string? QRCodeImagePath { get; set; }
        public DateOnly? PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public string? InvoiceNo { get; set; }
        public int? VendorId { get; set; }
        public string? VendorName { get; set; }
        public DateOnly? WarrantyExpiry { get; set; }
        public DateOnly? AMCExpiry { get; set; }
        public int? CurrentLocationId { get; set; }
        public string? LocationName { get; set; }
        public string? AssignedToType { get; set; }
        public int? AssignedToId { get; set; }
        public string UnitCondition { get; set; } = "Good";
        public bool IsAvailable { get; set; }
        public string? Remarks { get; set; }

        // Navigation
        public string? AssetName { get; set; }
        public string? CategoryName { get; set; }
        public bool IsWarrantyExpiringSoon { get; set; }
    }

    public class AddUnitsDto
    {
        public int AssetId { get; set; }
        public string? AssetName { get; set; }

        [Required, Range(1, 500)]
        public int NumberOfUnits { get; set; }

        public DateOnly? PurchaseDate { get; set; }

        [Required, Range(0.01, 9999999.99)]
        public decimal PurchasePrice { get; set; }

        [StringLength(100)]
        public string? InvoiceNo { get; set; }

        public int? VendorId { get; set; }
        public DateOnly? WarrantyExpiry { get; set; }
        public DateOnly? AMCExpiry { get; set; }
        public int? DefaultLocationId { get; set; }
    }

    // ============================================================
    // QR / SCAN
    // ============================================================
    public class AssetQRDataDto
    {
        public int UnitId { get; set; }
        public string AssetTag { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string? SubCategory { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? CurrentLocation { get; set; }
        public string ScanURL { get; set; } = null!;
    }

    public class AssetScanInfoDto
    {
        public int UnitId { get; set; }
        public string AssetTag { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public string AssetCode { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public string? SubCategoryName { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? CurrentLocation { get; set; }
        public string UnitCondition { get; set; } = null!;
        public bool IsAvailable { get; set; }
        public DateOnly? WarrantyExpiry { get; set; }
        public DateOnly? AMCExpiry { get; set; }

        // If issued
        public string? IssuedTo { get; set; }
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpectedReturnDate { get; set; }

        // Last maintenance
        public string? LastMaintenanceType { get; set; }
        public DateOnly? LastMaintenanceDate { get; set; }
    }

    // ============================================================
    // MOVE ASSET
    // ============================================================
    public class MoveAssetDto
    {
        public int UnitId { get; set; }
        public string? AssetTag { get; set; }
        public string? AssetName { get; set; }
        public string? CurrentLocationName { get; set; }
        public int? FromLocationId { get; set; }

        [Required]
        public int ToLocationId { get; set; }

        public DateTime MoveDate { get; set; } = DateTime.Now;

        [StringLength(300)]
        public string? Reason { get; set; }

        public int MovedBy { get; set; }
    }

    // ============================================================
    // ISSUE / RETURN
    // ============================================================
    public class IssueAssetDto
    {
        [Required]
        public string AssetTag { get; set; } = null!;

        public int UnitId { get; set; }
        public string? AssetName { get; set; }
        public string? CategoryName { get; set; }
        public string? CurrentLocation { get; set; }
        public string? UnitCondition { get; set; }

        [Required]
        public string IssuedToType { get; set; } = null!;

        [Required]
        public int IssuedToId { get; set; }

        public string? IssuedToName { get; set; }
        public int IssuedBy { get; set; }
        public DateOnly IssueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
        public DateOnly? ExpectedReturnDate { get; set; }

        [StringLength(300)]
        public string? Purpose { get; set; }

        public string TransactionStatus { get; set; } = "Issued";

        [StringLength(500)]
        public string? Remarks { get; set; }
    }

    public class ReturnAssetDto
    {
        [Required]
        public string AssetTag { get; set; } = null!;

        public int IssueId { get; set; }
        public int UnitId { get; set; }
        public string? AssetName { get; set; }
        public string? IssuedToName { get; set; }
        public DateOnly IssueDate { get; set; }
        public DateOnly? ExpectedReturnDate { get; set; }
        public int DaysWithMember { get; set; }

        [Required]
        public string ConditionOnReturn { get; set; } = "Good";

        public bool IsDamaged { get; set; }
        public bool IsLost { get; set; }
        public decimal DamageFine { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        public int ReturnedTo { get; set; }
    }

    public class AssetEligibilityDto
    {
        public bool IsEligible { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsIssuable { get; set; }
        public string UnitCondition { get; set; } = null!;
        public string? Message { get; set; }
        public AssetUnitDto? UnitDetails { get; set; }
    }

    // ============================================================
    // MAINTENANCE
    // ============================================================
    public class MaintenanceLogDto
    {
        public int MaintenanceId { get; set; }
        public int UnitId { get; set; }
        public string? AssetTag { get; set; }
        public string? AssetName { get; set; }

        [Required]
        public string MaintenanceType { get; set; } = null!;

        [Required, StringLength(500)]
        public string Description { get; set; } = null!;

        [StringLength(200)]
        public string? ServicedBy { get; set; }

        public int? VendorId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }

        public DateOnly? CompletionDate { get; set; }

        [Range(0, 9999999.99)]
        public decimal Cost { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        public string? ConditionBefore { get; set; }
        public string? ConditionAfter { get; set; }

        [StringLength(100)]
        public string? BillNo { get; set; }

        [StringLength(300)]
        public string? Remarks { get; set; }

        public int CreatedBy { get; set; }
    }

    // ============================================================
    // DAMAGE / LOSS REPORT
    // ============================================================
    public class DamageLossReportDto
    {
        public int ReportId { get; set; }
        public int UnitId { get; set; }
        public int? IssueId { get; set; }
        public string? AssetTag { get; set; }
        public string? AssetName { get; set; }

        [Required]
        public string ReportType { get; set; } = null!;

        public int ReportedBy { get; set; }
        public DateOnly ReportDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required, StringLength(500)]
        public string Description { get; set; } = null!;

        public string? ResponsibleType { get; set; }
        public int? ResponsibleId { get; set; }
        public string? ResponsibleName { get; set; }

        [Range(0, 9999999.99)]
        public decimal EstimatedLoss { get; set; }

        [Range(0, 9999999.99)]
        public decimal FineImposed { get; set; }

        public bool IsFinePaid { get; set; }
        public DateOnly? FinePaidDate { get; set; }

        [StringLength(300)]
        public string? ActionTaken { get; set; }

        public string Status { get; set; } = "Open";

        [StringLength(300)]
        public string? Remarks { get; set; }
    }

    // ============================================================
    // DISPOSAL
    // ============================================================
    public class DisposalDto
    {
        public int DisposalId { get; set; }
        public int UnitId { get; set; }
        public string? AssetTag { get; set; }
        public string? AssetName { get; set; }

        [Required]
        public string DisposalType { get; set; } = null!;

        [Required]
        public DateOnly DisposalDate { get; set; }

        [Range(0, 9999999.99)]
        public decimal SaleValue { get; set; }

        [StringLength(200)]
        public string? DisposedTo { get; set; }

        public int AuthorizedBy { get; set; }

        [Required, StringLength(500)]
        public string Reason { get; set; } = null!;

        [StringLength(300)]
        public string? Remarks { get; set; }

        public int CreatedBy { get; set; }
    }

    // ============================================================
    // DASHBOARD
    // ============================================================
    public class AssetAdminDashboardDto
    {
        public int TotalAssets { get; set; }
        public int AvailableUnits { get; set; }
        public int IssuedUnits { get; set; }
        public int UnderRepair { get; set; }
        public int OverdueReturns { get; set; }
        public int WarrantyExpiringSoon { get; set; }
        public int PendingMaintenance { get; set; }
        public int OpenDamageReports { get; set; }

        public List<CategoryStockDto> CategoryStock { get; set; } = new();
        public List<OverdueAssetDto> OverdueAssets { get; set; } = new();
        public List<WarrantyAlertDto> WarrantyAlerts { get; set; } = new();
        public List<MaintenanceAlertDto> PendingMaintenanceList { get; set; } = new();
        public List<RecentIssueDto> RecentIssues { get; set; } = new();
        public List<LocationStockDto> LocationStock { get; set; } = new();
    }

    public class AssetMemberDashboardDto
    {
        public List<MyIssuedAssetDto> MyIssuedAssets { get; set; } = new();
        public List<MyIssueHistoryDto> MyIssueHistory { get; set; } = new();
        public List<AssetUnitDto> LocationAssets { get; set; } = new();
    }

    public class CategoryStockDto
    {
        public string CategoryName { get; set; } = null!;
        public int Total { get; set; }
        public int Available { get; set; }
        public int Issued { get; set; }
    }

    public class OverdueAssetDto
    {
        public int IssueId { get; set; }
        public string AssetName { get; set; } = null!;
        public string AssetTag { get; set; } = null!;
        public string IssuedTo { get; set; } = null!;
        public string IssuedToType { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly ExpectedReturnDate { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class WarrantyAlertDto
    {
        public int UnitId { get; set; }
        public string AssetName { get; set; } = null!;
        public string AssetTag { get; set; } = null!;
        public string? LocationName { get; set; }
        public DateOnly WarrantyExpiry { get; set; }
        public int DaysLeft { get; set; }
    }

    public class MaintenanceAlertDto
    {
        public int MaintenanceId { get; set; }
        public string AssetName { get; set; } = null!;
        public string AssetTag { get; set; } = null!;
        public string MaintenanceType { get; set; } = null!;
        public DateOnly StartDate { get; set; }
        public string Status { get; set; } = null!;
    }

    public class RecentIssueDto
    {
        public int IssueId { get; set; }
        public string AssetName { get; set; } = null!;
        public string AssetTag { get; set; } = null!;
        public string IssuedTo { get; set; } = null!;
        public string IssuedToType { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly? ExpectedReturnDate { get; set; }
    }

    public class LocationStockDto
    {
        public string LocationName { get; set; } = null!;
        public int TotalAssets { get; set; }
        public int IssuedCount { get; set; }
    }

    public class MyIssuedAssetDto
    {
        public int IssueId { get; set; }
        public string AssetName { get; set; } = null!;
        public string AssetTag { get; set; } = null!;
        public string CategoryName { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly? ExpectedReturnDate { get; set; }
        public int? DaysRemaining { get; set; }
        public bool IsOverdue { get; set; }
        public string Status { get; set; } = null!;
    }

    public class MyIssueHistoryDto
    {
        public int IssueId { get; set; }
        public string AssetName { get; set; } = null!;
        public string AssetTag { get; set; } = null!;
        public DateOnly IssueDate { get; set; }
        public DateOnly? ReturnDate { get; set; }
        public string? ConditionOnReturn { get; set; }
        public decimal DamageFine { get; set; }
        public string TransactionStatus { get; set; } = null!;
    }

    // ============================================================
    // PERSON LOOKUP (for issue form)
    // ============================================================
    public class PersonLookupDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Code { get; set; } = null!;
        public string Type { get; set; } = null!;
    }
}
