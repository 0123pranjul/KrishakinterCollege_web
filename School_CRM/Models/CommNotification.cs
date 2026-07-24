using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommNotification
{
    public int NotificationId { get; set; }

    public string RecipientType { get; set; } = null!;

    public int RecipientId { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public string? RedirectUrl { get; set; }

    public int? ReferenceId { get; set; }

    public string? ReferenceType { get; set; }

    public string Priority { get; set; } = null!;

    public bool IsRead { get; set; }

    public DateTime? ReadAt { get; set; }

    public bool SendEmail { get; set; }

    public bool EmailSent { get; set; }

    public bool SendSms { get; set; }

    public bool Smssent { get; set; }

    public bool SendWhatsApp { get; set; }

    public bool WhatsAppSent { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }
}
