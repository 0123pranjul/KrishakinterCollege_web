using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnConductor
{
    public int ConductorId { get; set; }

    public string ConductorName { get; set; } = null!;

    public string ContactNumber { get; set; } = null!;

    public int? EmployeeId { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblTrnVehicleAssignment> TblTrnVehicleAssignments { get; set; } = new List<TblTrnVehicleAssignment>();
}
