using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnNotificationLog
{
    public int Id { get; set; }

    public int TripId { get; set; }

    public int StudentId { get; set; }

    public int StopId { get; set; }

    public DateTime NotificationSentAt { get; set; }

    public string NotificationChannel { get; set; } = null!;

    public string DeliveryStatus { get; set; } = null!;

    public virtual TblTrnRouteStop Stop { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;

    public virtual TblTrnTrip Trip { get; set; } = null!;
}
