using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class VwLibMemberBlockStatus
{
    public string UserType { get; set; } = null!;

    public int UserId { get; set; }

    public int? TotalBlocks { get; set; }

    public DateTime? LastBlockedAt { get; set; }

    public string? LatestBlockType { get; set; }

    public int IsCurrentlyBlocked { get; set; }
}
