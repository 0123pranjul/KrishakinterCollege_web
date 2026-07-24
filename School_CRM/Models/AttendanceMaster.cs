using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AttendanceMaster
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public DateOnly? AttendanceDate { get; set; }

    public string? Status { get; set; }

    public decimal? HoursWorked { get; set; }

    public decimal? OvertimeHours { get; set; }

    public TimeOnly? EntryTime { get; set; }

    public TimeOnly? ExitTime { get; set; }

    public virtual Employee? Employee { get; set; }
}
