using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblLessonCoverage
{
    public int Id { get; set; }

    public int LessonPlanId { get; set; }

    public DateOnly DateCovered { get; set; }

    public int PercentageCompleted { get; set; }

    public string? TeacherNotes { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual TblLessonPlan LessonPlan { get; set; } = null!;
}
