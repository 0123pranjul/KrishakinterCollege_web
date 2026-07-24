using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnVehicleAssignment
{
    public int AssignmentId { get; set; }

    public int RouteId { get; set; }

    public int VehicleId { get; set; }

    public int DriverId { get; set; }

    public int? ConductorId { get; set; }

    public DateOnly AssignedFrom { get; set; }

    public DateOnly AssignedTo { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblTrnConductor? Conductor { get; set; }

    public virtual TblTrnDriver Driver { get; set; } = null!;

    public virtual TblTrnRoute Route { get; set; } = null!;

    public virtual ICollection<TblTrnTrip> TblTrnTrips { get; set; } = new List<TblTrnTrip>();

    public virtual TblTrnVehicle Vehicle { get; set; } = null!;
}
