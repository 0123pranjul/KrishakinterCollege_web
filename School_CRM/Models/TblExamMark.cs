using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblExamMark
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    public int StudentId { get; set; }

    public int SubjectId { get; set; }

    public decimal? MarksObtained { get; set; }

    public bool? IsAbsent { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblExam Exam { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;
}
