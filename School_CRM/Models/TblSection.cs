using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblSection
{
    public int SectionId { get; set; }

    public string? SectionName { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblAssignment> TblAssignments { get; set; } = new List<TblAssignment>();

    public virtual ICollection<TblClassSection> TblClassSections { get; set; } = new List<TblClassSection>();

    public virtual ICollection<TblClassworkLog> TblClassworkLogs { get; set; } = new List<TblClassworkLog>();

    public virtual ICollection<TblCustomTest> TblCustomTests { get; set; } = new List<TblCustomTest>();

    public virtual ICollection<TblReportCard> TblReportCards { get; set; } = new List<TblReportCard>();

    public virtual ICollection<TblStudentAttendance> TblStudentAttendances { get; set; } = new List<TblStudentAttendance>();

    public virtual ICollection<TblStudentSession> TblStudentSessions { get; set; } = new List<TblStudentSession>();

    public virtual ICollection<TblStudyMaterial> TblStudyMaterials { get; set; } = new List<TblStudyMaterial>();

    public virtual ICollection<TblTeacherAssignment> TblTeacherAssignments { get; set; } = new List<TblTeacherAssignment>();

    public virtual ICollection<TblTimeTable> TblTimeTables { get; set; } = new List<TblTimeTable>();
}
