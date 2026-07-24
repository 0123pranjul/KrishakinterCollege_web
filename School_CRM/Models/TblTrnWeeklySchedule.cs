using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnWeeklySchedule
{
    public int Id { get; set; }

    public int RouteId { get; set; }

    public byte DayOfWeek { get; set; }

    public string TripType { get; set; } = null!;

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblTrnRoute Route { get; set; } = null!;
}
