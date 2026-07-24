using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblSyllabusTopic
{
    public int Id { get; set; }

    public int UnitId { get; set; }

    public string TopicName { get; set; } = null!;

    public int ExpectedPeriods { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblLessonPlan> TblLessonPlans { get; set; } = new List<TblLessonPlan>();

    public virtual TblSyllabusUnit Unit { get; set; } = null!;
}
