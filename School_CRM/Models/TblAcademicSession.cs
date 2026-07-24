using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblAcademicSession
{
    public int SessionId { get; set; }

    public string? SessionName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblAssignment> TblAssignments { get; set; } = new List<TblAssignment>();

    public virtual ICollection<TblClassSection> TblClassSections { get; set; } = new List<TblClassSection>();

    public virtual ICollection<TblEnquiry> TblEnquiries { get; set; } = new List<TblEnquiry>();

    public virtual ICollection<TblExamWeightage> TblExamWeightages { get; set; } = new List<TblExamWeightage>();

    public virtual ICollection<TblExam> TblExams { get; set; } = new List<TblExam>();

    public virtual ICollection<TblFeeCollection> TblFeeCollections { get; set; } = new List<TblFeeCollection>();

    public virtual ICollection<TblFeeStructure> TblFeeStructures { get; set; } = new List<TblFeeStructure>();

    public virtual ICollection<TblPromotionLog> TblPromotionLogSourceSessions { get; set; } = new List<TblPromotionLog>();

    public virtual ICollection<TblPromotionLog> TblPromotionLogTargetSessions { get; set; } = new List<TblPromotionLog>();

    public virtual ICollection<TblReportCard> TblReportCards { get; set; } = new List<TblReportCard>();

    public virtual ICollection<TblStudentAttendance> TblStudentAttendances { get; set; } = new List<TblStudentAttendance>();

    public virtual ICollection<TblStudentDue> TblStudentDues { get; set; } = new List<TblStudentDue>();

    public virtual ICollection<TblStudentExit> TblStudentExits { get; set; } = new List<TblStudentExit>();

    public virtual ICollection<TblStudentExtraCharge> TblStudentExtraCharges { get; set; } = new List<TblStudentExtraCharge>();

    public virtual ICollection<TblStudentOptionalFee> TblStudentOptionalFees { get; set; } = new List<TblStudentOptionalFee>();

    public virtual ICollection<TblStudentSession> TblStudentSessions { get; set; } = new List<TblStudentSession>();

    public virtual ICollection<TblTeacherAssignment> TblTeacherAssignments { get; set; } = new List<TblTeacherAssignment>();

    public virtual ICollection<TblTimeTable> TblTimeTables { get; set; } = new List<TblTimeTable>();

    public virtual ICollection<TblTrnRoute> TblTrnRoutes { get; set; } = new List<TblTrnRoute>();

    public virtual ICollection<TblTrnStudentAssignment> TblTrnStudentAssignments { get; set; } = new List<TblTrnStudentAssignment>();
}
