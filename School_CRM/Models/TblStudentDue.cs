using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentDue
{
    public int Id { get; set; }

    public int? StudentId { get; set; }

    public int? SessionId { get; set; }

    public decimal? TotalDue { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public decimal? PaidAmount { get; set; }

    public string? DueType { get; set; }

    public DateOnly? DueDate { get; set; }

    public bool? IsSettled { get; set; }

    public DateTime? SettledDate { get; set; }

    public string? Remarks { get; set; }

    public virtual TblAcademicSession? Session { get; set; }

    public virtual TblStudent? Student { get; set; }
}
