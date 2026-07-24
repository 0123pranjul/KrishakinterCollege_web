using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmAsset
{
    public int AssetId { get; set; }

    public string AssetName { get; set; } = null!;

    public string AssetCode { get; set; } = null!;

    public int CategoryId { get; set; }

    public int? SubCategoryId { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? Specifications { get; set; }

    public decimal UnitPrice { get; set; }

    public int TotalUnits { get; set; }

    public int AvailableUnits { get; set; }

    public bool IsIssuable { get; set; }

    public string? AssetImagePath { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual ICollection<AsmAssetUnit> AsmAssetUnits { get; set; } = new List<AsmAssetUnit>();

    public virtual AsmCategory Category { get; set; } = null!;

    public virtual AsmSubCategory? SubCategory { get; set; }
}
