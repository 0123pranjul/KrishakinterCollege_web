using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommNotificationTemplate
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string NotificationType { get; set; } = null!;

    public string TitleTemplate { get; set; } = null!;

    public string BodyTemplate { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<CommScheduledJob> CommScheduledJobs { get; set; } = new List<CommScheduledJob>();
}
