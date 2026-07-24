using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblPromotionLog
{
    public int Id { get; set; }

    public int SourceSessionId { get; set; }

    public int TargetSessionId { get; set; }

    public int ClassSectionCreatedCount { get; set; }

    public int TimetableCreatedCount { get; set; }

    public int PromotedCount { get; set; }

    public int FailedCount { get; set; }

    public int RetainedOtherCount { get; set; }

    public int PassoutCount { get; set; }

    public int LeftSchoolCount { get; set; }

    public int? ExecutedByUserId { get; set; }

    public DateTime ExecutedAt { get; set; }

    public string Status { get; set; } = null!;

    public int? RolledBackByUserId { get; set; }

    public DateTime? RolledBackAt { get; set; }

    public virtual TblAcademicSession SourceSession { get; set; } = null!;

    public virtual TblAcademicSession TargetSession { get; set; } = null!;

    public virtual ICollection<TblClassSection> TblClassSections { get; set; } = new List<TblClassSection>();

    public virtual ICollection<TblStudentExit> TblStudentExits { get; set; } = new List<TblStudentExit>();

    public virtual ICollection<TblStudentSession> TblStudentSessions { get; set; } = new List<TblStudentSession>();

    public virtual ICollection<TblTimeTable> TblTimeTables { get; set; } = new List<TblTimeTable>();
}
