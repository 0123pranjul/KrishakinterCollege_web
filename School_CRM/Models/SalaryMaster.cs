using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class SalaryMaster
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public string? MonthYear { get; set; }

    public int? PayableDays { get; set; }

    public int? PresentDays { get; set; }

    public int? HolidayDays { get; set; }

    public int? LeaveDays { get; set; }

    public int? LwpDays { get; set; }

    public decimal? OvertimeHours { get; set; }

    public decimal? BasicSalary { get; set; }

    public decimal? OvertimeAmount { get; set; }

    public decimal? GrossSalary { get; set; }

    public decimal? Deductions { get; set; }

    public decimal? NetSalary { get; set; }

    public string? Status { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public virtual Employee? Employee { get; set; }
}
