using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentSession
{
    public int Id { get; set; }

    public int? StudentId { get; set; }

    public int? SessionId { get; set; }

    public int? ClassId { get; set; }

    public int? SectionId { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? PromotionAction { get; set; }

    public string? RetentionReason { get; set; }

    public string? RetentionRemarks { get; set; }

    public int? PromotionLogId { get; set; }

    public virtual TblClass? Class { get; set; }

    public virtual TblPromotionLog? PromotionLog { get; set; }

    public virtual TblSection? Section { get; set; }

    public virtual TblAcademicSession? Session { get; set; }

    public virtual TblStudent? Student { get; set; }
}
