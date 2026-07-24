using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTeacher
{
    public int TeacherId { get; set; }

    public string TeacherName { get; set; } = null!;

    public string? MobileNo { get; set; }

    public string? Email { get; set; }

    public string? Designation { get; set; }

    public DateOnly? JoiningDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblAssignment> TblAssignments { get; set; } = new List<TblAssignment>();

    public virtual ICollection<TblCustomTest> TblCustomTests { get; set; } = new List<TblCustomTest>();

    public virtual ICollection<TblStudyMaterial> TblStudyMaterials { get; set; } = new List<TblStudyMaterial>();

    public virtual ICollection<TblTeacherAssignment> TblTeacherAssignments { get; set; } = new List<TblTeacherAssignment>();

    public virtual ICollection<TblTimeTable> TblTimeTables { get; set; } = new List<TblTimeTable>();

    public virtual ICollection<UserMaster> UserMasters { get; set; } = new List<UserMaster>();
}
