using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmSubCategory
{
    public int SubCategoryId { get; set; }

    public int CategoryId { get; set; }

    public string SubCategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<AsmAsset> AsmAssets { get; set; } = new List<AsmAsset>();

    public virtual AsmCategory Category { get; set; } = null!;
}
