using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblFeeType
{
    public int FeeTypeId { get; set; }

    public string? FeeName { get; set; }

    public bool? IsRecurring { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? FeeCategory { get; set; }

    public virtual ICollection<TblFeeCollectionDetail> TblFeeCollectionDetails { get; set; } = new List<TblFeeCollectionDetail>();

    public virtual ICollection<TblFeeStructure> TblFeeStructures { get; set; } = new List<TblFeeStructure>();

    public virtual ICollection<TblStudentExtraCharge> TblStudentExtraCharges { get; set; } = new List<TblStudentExtraCharge>();

    public virtual ICollection<TblStudentFeeOverride> TblStudentFeeOverrides { get; set; } = new List<TblStudentFeeOverride>();

    public virtual ICollection<TblStudentOptionalFee> TblStudentOptionalFees { get; set; } = new List<TblStudentOptionalFee>();
}
