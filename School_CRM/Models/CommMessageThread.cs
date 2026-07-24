using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommMessageThread
{
    public int ThreadId { get; set; }

    public string InitiatorType { get; set; } = null!;

    public int InitiatorId { get; set; }

    public string RecipientType { get; set; } = null!;

    public int RecipientId { get; set; }

    public string? Subject { get; set; }

    public int? StudentId { get; set; }

    public DateTime LastMessageAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CommMessage> CommMessages { get; set; } = new List<CommMessage>();
}
