using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class VwLibCurrentIssue
{
    public int IssueId { get; set; }

    public string UserType { get; set; } = null!;

    public int UserId { get; set; }

    public int CopyId { get; set; }

    public string AccessionNo { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public DateOnly IssueDate { get; set; }

    public DateOnly DueDate { get; set; }

    public bool IsReturned { get; set; }

    public decimal FineAmount { get; set; }

    public bool IsFinePaid { get; set; }

    public string TransactionStatus { get; set; } = null!;

    public int? OverdueDaysToday { get; set; }

    public int IsOverdueNow { get; set; }
}
