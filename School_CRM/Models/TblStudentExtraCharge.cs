using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentExtraCharge
{
    public int Id { get; set; }

    public int? StudentId { get; set; }

    public int? SessionId { get; set; }

    public int? FeeTypeId { get; set; }

    public decimal? Amount { get; set; }

    public string? Reason { get; set; }

    public DateOnly? ChargeDate { get; set; }

    public bool? IsPaid { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblFeeType? FeeType { get; set; }

    public virtual TblAcademicSession? Session { get; set; }

    public virtual TblStudent? Student { get; set; }
}
