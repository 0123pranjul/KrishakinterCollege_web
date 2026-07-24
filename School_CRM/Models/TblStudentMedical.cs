using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentMedical
{
    public int Id { get; set; }

    public int? StudentId { get; set; }

    public string? Allergies { get; set; }

    public string? MedicalCondition { get; set; }

    public string? DoctorName { get; set; }

    public string? EmergencyContact { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblStudent? Student { get; set; }
}
