using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibIssueTransaction
{
    public int IssueId { get; set; }

    public string UserType { get; set; } = null!;

    public int UserId { get; set; }

    public int CopyId { get; set; }

    public int PolicyId { get; set; }

    public int IssuedBy { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly DueDate { get; set; }

    public DateOnly? ReturnDate { get; set; }

    public bool IsReturned { get; set; }

    public int? ReturnedTo { get; set; }

    public string? ConditionOnReturn { get; set; }

    public int? OverdueDays { get; set; }

    public decimal FineAmount { get; set; }

    public string? FineType { get; set; }

    public bool IsFinePaid { get; set; }

    public DateTime? FinePaidDate { get; set; }

    public int? FinePaidBy { get; set; }

    public string TransactionStatus { get; set; } = null!;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual LibBookCopy Copy { get; set; } = null!;

    public virtual ICollection<LibFinePayment> LibFinePayments { get; set; } = new List<LibFinePayment>();

    public virtual ICollection<LibMemberBlockLog> LibMemberBlockLogs { get; set; } = new List<LibMemberBlockLog>();

    public virtual LibFinePolicy Policy { get; set; } = null!;
}
