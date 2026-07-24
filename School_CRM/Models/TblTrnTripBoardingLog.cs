using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnTripBoardingLog
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public int StudentId { get; set; }

    public int StopId { get; set; }

    public string BoardingStatus { get; set; } = null!;

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual TblTrnRouteStop Stop { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;

    public virtual TblTrnTrip Trip { get; set; } = null!;
}
