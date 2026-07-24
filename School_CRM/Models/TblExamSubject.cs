using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblExamSubject
{
    public int Id { get; set; }

    public int ExamId { get; set; }

    public int ClassId { get; set; }

    public int SubjectId { get; set; }

    public decimal MaxMarks { get; set; }

    public decimal PassMarks { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public DateOnly? ExamDate { get; set; }

    public string? ExamTime { get; set; }

    public string? RoomNo { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual TblExam Exam { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;
}
