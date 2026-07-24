using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentParent
{
    public int ParentId { get; set; }

    public int? StudentId { get; set; }

    public string? ParentName { get; set; }

    public string? ParentType { get; set; }

    public string? MobileNo { get; set; }

    public string? AlternateMobile { get; set; }

    public string? Email { get; set; }

    public string? Occupation { get; set; }

    public bool? IsPrimary { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblStudent? Student { get; set; }
}
