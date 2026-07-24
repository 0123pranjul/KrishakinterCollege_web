using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<AsmAsset> AsmAssets { get; set; } = new List<AsmAsset>();

    public virtual ICollection<AsmSubCategory> AsmSubCategories { get; set; } = new List<AsmSubCategory>();
}
