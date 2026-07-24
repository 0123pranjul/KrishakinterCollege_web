using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblAssignment
{
    public int AssignmentId { get; set; }

    public int TeacherId { get; set; }

    public int ClassId { get; set; }

    public int SectionId { get; set; }

    public int SubjectId { get; set; }

    public int SessionId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly DueDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual TblSection Section { get; set; } = null!;

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;

    public virtual TblTeacher Teacher { get; set; } = null!;
}
