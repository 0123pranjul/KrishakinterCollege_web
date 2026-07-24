using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibMemberBlockLog
{
    public int BlockId { get; set; }

    public string UserType { get; set; } = null!;

    public int UserId { get; set; }

    public string BlockReason { get; set; } = null!;

    public string BlockType { get; set; } = null!;

    public int? IssueId { get; set; }

    public bool IsBlocked { get; set; }

    public int BlockedBy { get; set; }

    public DateTime BlockedAt { get; set; }

    public int? UnblockedBy { get; set; }

    public DateTime? UnblockedAt { get; set; }

    public string? UnblockReason { get; set; }

    public virtual LibIssueTransaction? Issue { get; set; }
}
