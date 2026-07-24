using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmAssetUnit
{
    public int UnitId { get; set; }

    public int AssetId { get; set; }

    public string AssetTag { get; set; } = null!;

    public string? QrcodeData { get; set; }

    public string? QrcodeImagePath { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public decimal PurchasePrice { get; set; }

    public string? InvoiceNo { get; set; }

    public int? VendorId { get; set; }

    public DateOnly? WarrantyExpiry { get; set; }

    public DateOnly? Amcexpiry { get; set; }

    public string? Amcvendor { get; set; }

    public int? CurrentLocationId { get; set; }

    public string? AssignedToType { get; set; }

    public int? AssignedToId { get; set; }

    public string UnitCondition { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public bool IsActive { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<AsmDamageLossReport> AsmDamageLossReports { get; set; } = new List<AsmDamageLossReport>();

    public virtual ICollection<AsmDisposalLog> AsmDisposalLogs { get; set; } = new List<AsmDisposalLog>();

    public virtual ICollection<AsmIssueTransaction> AsmIssueTransactions { get; set; } = new List<AsmIssueTransaction>();

    public virtual ICollection<AsmLocationHistory> AsmLocationHistories { get; set; } = new List<AsmLocationHistory>();

    public virtual ICollection<AsmMaintenanceLog> AsmMaintenanceLogs { get; set; } = new List<AsmMaintenanceLog>();

    public virtual AsmAsset Asset { get; set; } = null!;

    public virtual AsmLocation? CurrentLocation { get; set; }

    public virtual AsmVendor? Vendor { get; set; }
}
