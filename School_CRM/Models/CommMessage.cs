using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommMessage
{
    public int MessageId { get; set; }

    public int ThreadId { get; set; }

    public string SenderType { get; set; } = null!;

    public int SenderId { get; set; }

    public string MessageBody { get; set; } = null!;

    public string? AttachmentPath { get; set; }

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public DateTime SentAt { get; set; }

    public bool IsDeleted { get; set; }

    public virtual CommMessageThread Thread { get; set; } = null!;
}
