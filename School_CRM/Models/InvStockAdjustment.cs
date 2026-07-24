using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvStockAdjustment
{
    public int AdjustmentId { get; set; }

    public int ProductId { get; set; }

    public string AdjustmentType { get; set; } = null!;

    public int QuantityBefore { get; set; }

    public int AdjustedQty { get; set; }

    public int? QuantityAfter { get; set; }

    public string Reason { get; set; } = null!;

    public int AdjustedBy { get; set; }

    public DateTime AdjustedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public string? Remarks { get; set; }

    public virtual InvProduct Product { get; set; } = null!;
}
