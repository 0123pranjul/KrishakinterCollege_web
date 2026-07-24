using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmMaintenanceLog
{
    public int MaintenanceId { get; set; }

    public int UnitId { get; set; }

    public string MaintenanceType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? ServicedBy { get; set; }

    public int? VendorId { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public decimal Cost { get; set; }

    public string Status { get; set; } = null!;

    public string? ConditionBefore { get; set; }

    public string? ConditionAfter { get; set; }

    public string? BillNo { get; set; }

    public string? Remarks { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AsmAssetUnit Unit { get; set; } = null!;

    public virtual AsmVendor? Vendor { get; set; }
}
