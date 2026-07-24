using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class EmployeeAdvance
{
    public int Id { get; set; }

    public int? EmployeeId { get; set; }

    public DateTime? AdvanceDate { get; set; }

    public decimal Amount { get; set; }

    public string? Reason { get; set; }

    public string? DeductFromMonth { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedDate { get; set; }

    public virtual Employee? Employee { get; set; }
}
