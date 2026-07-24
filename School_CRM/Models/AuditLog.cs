using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AuditLog
{
    public long AuditId { get; set; }

    public string TableName { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? RecordId { get; set; }

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public int? ChangedByUserId { get; set; }

    public string? ChangedByName { get; set; }

    public string? UserRole { get; set; }

    public string? IpAddress { get; set; }

    public string? ControllerName { get; set; }

    public string? ActionName { get; set; }

    public string? RequestUrl { get; set; }

    public DateTime ChangedAt { get; set; }

    public string? Remarks { get; set; }
}
