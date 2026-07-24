using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblAnnouncement
{
    public int AnnouncementId { get; set; }

    public string Title { get; set; } = null!;

    public string Message { get; set; } = null!;

    public int? ClassId { get; set; }

    public int? SectionId { get; set; }

    public bool? IsGlobal { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
