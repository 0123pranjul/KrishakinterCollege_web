using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class EmployeeLeaf
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public string? LeaveType { get; set; }

    public DateOnly? FromDate { get; set; }

    public DateOnly? ToDate { get; set; }

    public decimal? TotalDays { get; set; }

    public string? Status { get; set; }

    public virtual Employee? Employee { get; set; }
}
