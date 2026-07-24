using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblEnquiry
{
    public int EnquiryId { get; set; }

    public string? StudentName { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? ParentName { get; set; }

    public string? MobileNo { get; set; }

    public string? AlternateMobile { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public int? InterestedClassId { get; set; }

    public int? SessionId { get; set; }

    public DateTime? EnquiryDate { get; set; }

    public string? Source { get; set; }

    public string? Status { get; set; }

    public string? Remarks { get; set; }

    public int? AssignedTo { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblClass? InterestedClass { get; set; }

    public virtual TblAcademicSession? Session { get; set; }

    public virtual ICollection<TblEnquiryDocument> TblEnquiryDocuments { get; set; } = new List<TblEnquiryDocument>();

    public virtual ICollection<TblEnquiryFollowUp> TblEnquiryFollowUps { get; set; } = new List<TblEnquiryFollowUp>();
}
