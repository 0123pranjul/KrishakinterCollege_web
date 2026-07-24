using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmVendor
{
    public int VendorId { get; set; }

    public string VendorName { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Gstno { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<AsmAssetUnit> AsmAssetUnits { get; set; } = new List<AsmAssetUnit>();

    public virtual ICollection<AsmMaintenanceLog> AsmMaintenanceLogs { get; set; } = new List<AsmMaintenanceLog>();
}
