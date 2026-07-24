using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmLocationHistory
{
    public int HistoryId { get; set; }

    public int UnitId { get; set; }

    public int? FromLocationId { get; set; }

    public int ToLocationId { get; set; }

    public int MovedBy { get; set; }

    public DateTime MoveDate { get; set; }

    public string? Reason { get; set; }

    public virtual AsmLocation? FromLocation { get; set; }

    public virtual AsmLocation ToLocation { get; set; } = null!;

    public virtual AsmAssetUnit Unit { get; set; } = null!;
}
