using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommAnnouncementRead
{
    public int ReadId { get; set; }

    public int AnnouncementId { get; set; }

    public string ReaderType { get; set; } = null!;

    public int ReaderId { get; set; }

    public DateTime ReadAt { get; set; }

    public virtual CommAnnouncement Announcement { get; set; } = null!;
}
