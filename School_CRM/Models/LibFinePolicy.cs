using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibFinePolicy
{
    public int PolicyId { get; set; }

    public string PolicyName { get; set; } = null!;

    public decimal PerDayFine { get; set; }

    public int GracePeriodDays { get; set; }

    public decimal? MaxOverdueFine { get; set; }

    public int MaxBooksForStudent { get; set; }

    public int MaxBooksForTeacher { get; set; }

    public int IssueDaysForStudent { get; set; }

    public int IssueDaysForTeacher { get; set; }

    public string DamageFineType { get; set; } = null!;

    public decimal DamageFineValue { get; set; }

    public string LostFineType { get; set; } = null!;

    public decimal LostFineValue { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<LibIssueTransaction> LibIssueTransactions { get; set; } = new List<LibIssueTransaction>();
}
