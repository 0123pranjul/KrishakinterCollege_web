using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblEnquiryDocument
{
    public int DocumentId { get; set; }

    public int? EnquiryId { get; set; }

    public string? DocumentType { get; set; }

    public string? DocumentUrl { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblEnquiry? Enquiry { get; set; }
}
