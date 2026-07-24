using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnGpsUpdate
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public DateTime ReceivedAt { get; set; }

    public DateTime? DeviceTimestamp { get; set; }

    public virtual TblTrnTrip Trip { get; set; } = null!;
}
