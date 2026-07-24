using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentOptionalFee
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int SessionId { get; set; }

    public int FeeTypeId { get; set; }

    public decimal Amount { get; set; }

    public string? Remarks { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblFeeType FeeType { get; set; } = null!;

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;
}
