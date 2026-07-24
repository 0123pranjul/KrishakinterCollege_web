using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblGradeMaster
{
    public int GradeId { get; set; }

    public string GradeName { get; set; } = null!;

    public decimal MinPercent { get; set; }

    public decimal MaxPercent { get; set; }

    public decimal GradePoint { get; set; }

    public string? Remark { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblReportCardSubject> TblReportCardSubjects { get; set; } = new List<TblReportCardSubject>();

    public virtual ICollection<TblReportCard> TblReportCards { get; set; } = new List<TblReportCard>();
}
