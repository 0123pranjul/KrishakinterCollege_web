using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmDisposalLog
{
    public int DisposalId { get; set; }

    public int UnitId { get; set; }

    public string DisposalType { get; set; } = null!;

    public DateOnly DisposalDate { get; set; }

    public decimal SaleValue { get; set; }

    public string? DisposedTo { get; set; }

    public int AuthorizedBy { get; set; }

    public string Reason { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual AsmAssetUnit Unit { get; set; } = null!;
}
