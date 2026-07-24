using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblSubject
{
    public int SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblAssignment> TblAssignments { get; set; } = new List<TblAssignment>();

    public virtual ICollection<TblClassSubject> TblClassSubjects { get; set; } = new List<TblClassSubject>();

    public virtual ICollection<TblClassworkLog> TblClassworkLogs { get; set; } = new List<TblClassworkLog>();

    public virtual ICollection<TblCustomTest> TblCustomTests { get; set; } = new List<TblCustomTest>();

    public virtual ICollection<TblExamMark> TblExamMarks { get; set; } = new List<TblExamMark>();

    public virtual ICollection<TblExamSubject> TblExamSubjects { get; set; } = new List<TblExamSubject>();

    public virtual ICollection<TblLessonPlan> TblLessonPlans { get; set; } = new List<TblLessonPlan>();

    public virtual ICollection<TblReportCardSubject> TblReportCardSubjects { get; set; } = new List<TblReportCardSubject>();

    public virtual ICollection<TblStudyMaterial> TblStudyMaterials { get; set; } = new List<TblStudyMaterial>();

    public virtual ICollection<TblSyllabusUnit> TblSyllabusUnits { get; set; } = new List<TblSyllabusUnit>();

    public virtual ICollection<TblTeacherAssignment> TblTeacherAssignments { get; set; } = new List<TblTeacherAssignment>();

    public virtual ICollection<TblTimeTable> TblTimeTables { get; set; } = new List<TblTimeTable>();
}
