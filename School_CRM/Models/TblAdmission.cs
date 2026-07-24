using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblAdmission
{
    public int AdmissionId { get; set; }

    public int? StudentId { get; set; }

    public int? SessionId { get; set; }

    public int? ClassId { get; set; }

    public int? SectionId { get; set; }

    public DateOnly? AdmissionDate { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public string? AdmissionType { get; set; }

    public string? AdmissionStatus { get; set; }

    public string? Remarks { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblStudent? Student { get; set; }
}
