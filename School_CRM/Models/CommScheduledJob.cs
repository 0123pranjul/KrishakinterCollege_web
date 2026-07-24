using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommScheduledJob
{
    public int JobId { get; set; }

    public string JobName { get; set; } = null!;

    public int TemplateId { get; set; }

    public string ScheduleType { get; set; } = null!;

    public int? RunOnDay { get; set; }

    public TimeOnly RunTime { get; set; }

    public DateTime? NextRunAt { get; set; }

    public DateTime? LastRunAt { get; set; }

    public string TargetType { get; set; } = null!;

    public int? TargetClassId { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual CommNotificationTemplate Template { get; set; } = null!;
}
