using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblFeeTransaction
{
    public int TransactionId { get; set; }

    public int? FeeCollectionId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaymentMode { get; set; }

    public DateTime? TransactionDate { get; set; }

    public string? ReferenceNo { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblFeeCollection? FeeCollection { get; set; }
}
