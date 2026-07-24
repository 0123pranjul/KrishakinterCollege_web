using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblFeeCollectionDetail
{
    public int Id { get; set; }

    public int? FeeCollectionId { get; set; }

    public int? FeeTypeId { get; set; }

    public decimal? Amount { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblFeeCollection? FeeCollection { get; set; }

    public virtual TblFeeType? FeeType { get; set; }
}
