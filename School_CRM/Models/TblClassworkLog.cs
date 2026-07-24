using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblClassworkLog
{
    public int Id { get; set; }

    public DateOnly LogDate { get; set; }

    public int ClassId { get; set; }

    public int SectionId { get; set; }

    public int SubjectId { get; set; }

    public int EmployeeId { get; set; }

    public string TopicCovered { get; set; } = null!;

    public string? Remarks { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual TblSection Section { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;
}
