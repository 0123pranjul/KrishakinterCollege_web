using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblLessonPlan
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int ClassId { get; set; }

    public int SubjectId { get; set; }

    public int? TopicId { get; set; }

    public string PlanTitle { get; set; } = null!;

    public string Objectives { get; set; } = null!;

    public string TeachingMethod { get; set; } = null!;

    public string? RequiredMaterials { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    public string Status { get; set; } = null!;

    public string? ReviewRemarks { get; set; }

    public int? ReviewedBy { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual Employee Employee { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;

    public virtual ICollection<TblLessonCoverage> TblLessonCoverages { get; set; } = new List<TblLessonCoverage>();

    public virtual TblSyllabusTopic? Topic { get; set; }
}
