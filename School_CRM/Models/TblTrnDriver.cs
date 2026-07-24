using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnDriver
{
    public int DriverId { get; set; }

    public string DriverName { get; set; } = null!;

    public string ContactNumber { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public DateOnly? LicenseExpiry { get; set; }

    public int? EmployeeId { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblTrnVehicleAssignment> TblTrnVehicleAssignments { get; set; } = new List<TblTrnVehicleAssignment>();
}
