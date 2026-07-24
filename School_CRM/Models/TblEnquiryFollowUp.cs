using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblEnquiryFollowUp
{
    public int FollowUpId { get; set; }

    public int? EnquiryId { get; set; }

    public DateTime? FollowUpDate { get; set; }

    public DateTime? NextFollowUpDate { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblEnquiry? Enquiry { get; set; }
}
