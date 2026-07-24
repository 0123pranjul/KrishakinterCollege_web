using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmIssueTransaction
{
    public int IssueId { get; set; }

    public int UnitId { get; set; }

    public string IssuedToType { get; set; } = null!;

    public int IssuedToId { get; set; }

    public int IssuedBy { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly? ExpectedReturnDate { get; set; }

    public string? Purpose { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public bool IsReturned { get; set; }

    public int? ReturnedTo { get; set; }

    public string ConditionOnIssue { get; set; } = null!;

    public string? ConditionOnReturn { get; set; }

    public bool IsDamaged { get; set; }

    public bool IsLost { get; set; }

    public decimal DamageFine { get; set; }

    public bool IsFinePaid { get; set; }

    public string TransactionStatus { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AsmDamageLossReport> AsmDamageLossReports { get; set; } = new List<AsmDamageLossReport>();

    public virtual AsmAssetUnit Unit { get; set; } = null!;
}
