using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnRoute
{
    public int RouteId { get; set; }

    public string RouteName { get; set; } = null!;

    public int SessionId { get; set; }

    public short MaxStudentCapacity { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual ICollection<TblTrnRouteStop> TblTrnRouteStops { get; set; } = new List<TblTrnRouteStop>();

    public virtual ICollection<TblTrnStudentAssignment> TblTrnStudentAssignments { get; set; } = new List<TblTrnStudentAssignment>();

    public virtual ICollection<TblTrnTrip> TblTrnTrips { get; set; } = new List<TblTrnTrip>();

    public virtual ICollection<TblTrnVehicleAssignment> TblTrnVehicleAssignments { get; set; } = new List<TblTrnVehicleAssignment>();

    public virtual ICollection<TblTrnWeeklySchedule> TblTrnWeeklySchedules { get; set; } = new List<TblTrnWeeklySchedule>();
}
