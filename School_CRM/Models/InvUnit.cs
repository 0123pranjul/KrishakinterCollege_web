using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvUnit
{
    public int UnitId { get; set; }

    public string UnitName { get; set; } = null!;

    public string UnitShort { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<InvProduct> InvProducts { get; set; } = new List<InvProduct>();
}
