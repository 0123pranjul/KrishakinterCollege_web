using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblFeeCollection
{
    public int FeeCollectionId { get; set; }

    public int? StudentId { get; set; }

    public int? SessionId { get; set; }

    public int? Month { get; set; }

    public int? Year { get; set; }

    public decimal? TotalAmount { get; set; }

    public decimal? PaidAmount { get; set; }

    public decimal? DiscountAmount { get; set; }

    public decimal? FineAmount { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? PaymentMode { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblAcademicSession? Session { get; set; }

    public virtual TblStudent? Student { get; set; }

    public virtual ICollection<TblFeeCollectionDetail> TblFeeCollectionDetails { get; set; } = new List<TblFeeCollectionDetail>();

    public virtual ICollection<TblFeeTransaction> TblFeeTransactions { get; set; } = new List<TblFeeTransaction>();
}
