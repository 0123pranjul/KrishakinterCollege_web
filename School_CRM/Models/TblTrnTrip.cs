using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnTrip
{
    public int TripId { get; set; }

    public int RouteId { get; set; }

    public int? AssignmentId { get; set; }

    public DateOnly TripDate { get; set; }

    public string TripType { get; set; } = null!;

    public string TripStatus { get; set; } = null!;

    public DateTime? ActualStartTime { get; set; }

    public DateTime? ActualEndTime { get; set; }

    public string? AdherenceStatus { get; set; }

    public string SecureToken { get; set; } = null!;

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblTrnVehicleAssignment? Assignment { get; set; }

    public virtual TblTrnRoute Route { get; set; } = null!;

    public virtual ICollection<TblTrnGpsUpdate> TblTrnGpsUpdates { get; set; } = new List<TblTrnGpsUpdate>();

    public virtual ICollection<TblTrnNotificationLog> TblTrnNotificationLogs { get; set; } = new List<TblTrnNotificationLog>();

    public virtual ICollection<TblTrnTripBoardingLog> TblTrnTripBoardingLogs { get; set; } = new List<TblTrnTripBoardingLog>();
}
