using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmLocation
{
    public int LocationId { get; set; }

    public string LocationName { get; set; } = null!;

    public string LocationType { get; set; } = null!;

    public string? Floor { get; set; }

    public string? Building { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<AsmAssetUnit> AsmAssetUnits { get; set; } = new List<AsmAssetUnit>();

    public virtual ICollection<AsmLocationHistory> AsmLocationHistoryFromLocations { get; set; } = new List<AsmLocationHistory>();

    public virtual ICollection<AsmLocationHistory> AsmLocationHistoryToLocations { get; set; } = new List<AsmLocationHistory>();
}
