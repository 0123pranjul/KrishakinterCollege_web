using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblReportCard
{
    public int ReportCardId { get; set; }

    public int StudentId { get; set; }

    public int SessionId { get; set; }

    public int ClassId { get; set; }

    public int SectionId { get; set; }

    public decimal TotalMarks { get; set; }

    public decimal ObtainedMarks { get; set; }

    public decimal Percentage { get; set; }

    public int GradeId { get; set; }

    public int? Rank { get; set; }

    public bool? IsPublished { get; set; }

    public DateTime? PublishedDate { get; set; }

    public DateTime? GeneratedDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? VerificationCode { get; set; }

    public string? TeacherRemark { get; set; }

    public string? ResultStatus { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual TblGradeMaster Grade { get; set; } = null!;

    public virtual TblSection Section { get; set; } = null!;

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;

    public virtual ICollection<TblReportCardSubject> TblReportCardSubjects { get; set; } = new List<TblReportCardSubject>();
}
