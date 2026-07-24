using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblReportCardSubject
{
    public int Id { get; set; }

    public int ReportCardId { get; set; }

    public int SubjectId { get; set; }

    public decimal MaxMarks { get; set; }

    public decimal ObtainedMarks { get; set; }

    public decimal Percentage { get; set; }

    public int GradeId { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblGradeMaster Grade { get; set; } = null!;

    public virtual TblReportCard ReportCard { get; set; } = null!;

    public virtual TblSubject Subject { get; set; } = null!;
}
