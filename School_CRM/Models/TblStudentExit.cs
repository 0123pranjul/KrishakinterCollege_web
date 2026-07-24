using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentExit
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int SessionId { get; set; }

    public string ExitReason { get; set; } = null!;

    public DateOnly ExitDate { get; set; }

    public string? Remarks { get; set; }

    public int? PromotionLogId { get; set; }

    public bool IsActive { get; set; }

    public int? RecordedByUserId { get; set; }

    public DateTime RecordedAt { get; set; }

    public virtual TblPromotionLog? PromotionLog { get; set; }

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;
}
