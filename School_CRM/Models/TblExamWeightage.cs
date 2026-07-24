using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblExamWeightage
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ExamId { get; set; }

    public decimal WeightPct { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblExam Exam { get; set; } = null!;

    public virtual TblAcademicSession Session { get; set; } = null!;
}
