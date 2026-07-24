using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibFinePayment
{
    public int PaymentId { get; set; }

    public int IssueId { get; set; }

    public string UserType { get; set; } = null!;

    public int UserId { get; set; }

    public decimal AmountPaid { get; set; }

    public string PaymentMode { get; set; } = null!;

    public DateTime PaymentDate { get; set; }

    public int CollectedBy { get; set; }

    public string? ReceiptNo { get; set; }

    public string? Remarks { get; set; }

    public virtual LibIssueTransaction Issue { get; set; } = null!;
}
