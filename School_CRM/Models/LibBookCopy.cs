using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibBookCopy
{
    public int CopyId { get; set; }

    public int BookId { get; set; }

    public string AccessionNo { get; set; } = null!;

    public string? QrcodeData { get; set; }

    public string? QrcodeImagePath { get; set; }

    public string CopyCondition { get; set; } = null!;

    public bool IsAvailable { get; set; }

    public DateOnly AcquisitionDate { get; set; }

    public decimal CopyPrice { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public string? Remarks { get; set; }

    public virtual LibBook Book { get; set; } = null!;

    public virtual ICollection<LibIssueTransaction> LibIssueTransactions { get; set; } = new List<LibIssueTransaction>();
}
