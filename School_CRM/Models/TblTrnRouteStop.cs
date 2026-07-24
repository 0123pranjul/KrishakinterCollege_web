using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnRouteStop
{
    public int StopId { get; set; }

    public int RouteId { get; set; }

    public string StopName { get; set; } = null!;

    public short StopOrder { get; set; }

    public TimeOnly ScheduledArrivalTime { get; set; }

    public TimeOnly ScheduledDepartureTime { get; set; }

    public decimal FareAmount { get; set; }

    public decimal? Latitude { get; set; }

    public decimal? Longitude { get; set; }

    public string? Landmark { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblTrnRoute Route { get; set; } = null!;

    public virtual ICollection<TblTrnNotificationLog> TblTrnNotificationLogs { get; set; } = new List<TblTrnNotificationLog>();

    public virtual ICollection<TblTrnStudentAssignment> TblTrnStudentAssignments { get; set; } = new List<TblTrnStudentAssignment>();

    public virtual ICollection<TblTrnTripBoardingLog> TblTrnTripBoardingLogs { get; set; } = new List<TblTrnTripBoardingLog>();
}
