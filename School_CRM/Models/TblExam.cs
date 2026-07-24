using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblExam
{
    public int ExamId { get; set; }

    public string ExamName { get; set; } = null!;

    public int SessionId { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual ICollection<TblExamMark> TblExamMarks { get; set; } = new List<TblExamMark>();

    public virtual ICollection<TblExamSubject> TblExamSubjects { get; set; } = new List<TblExamSubject>();

    public virtual ICollection<TblExamWeightage> TblExamWeightages { get; set; } = new List<TblExamWeightage>();
}
