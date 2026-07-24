using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommAnnouncement
{
    public int AnnouncementId { get; set; }

    public string Title { get; set; } = null!;

    public string Body { get; set; } = null!;

    public string TargetType { get; set; } = null!;

    public int? TargetClassId { get; set; }

    public int? TargetSectionId { get; set; }

    public int? TargetRoleId { get; set; }

    public string Priority { get; set; } = null!;

    public string? AttachmentPath { get; set; }

    public string? AttachmentName { get; set; }

    public DateTime PublishAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool IsPublished { get; set; }

    public bool IsPinned { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<CommAnnouncementRead> CommAnnouncementReads { get; set; } = new List<CommAnnouncementRead>();
}
