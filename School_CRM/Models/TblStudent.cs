using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudent
{
    public int StudentId { get; set; }

    public string? StudentName { get; set; }

    public string? RollNo { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public string? AdmissionNo { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public string? BloodGroup { get; set; }

    public string? AadhaarNo { get; set; }

    public DateOnly? AdmissionDate { get; set; }

    public string? PreviousSchool { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Pincode { get; set; }

    public string? EmergencyContactName { get; set; }

    public string? EmergencyContactNumber { get; set; }

    public string? PhotoUrl { get; set; }

    public string? Username { get; set; }

    public string? Password { get; set; }

    public virtual ICollection<TblAdmission> TblAdmissions { get; set; } = new List<TblAdmission>();

    public virtual ICollection<TblCustomTestMark> TblCustomTestMarks { get; set; } = new List<TblCustomTestMark>();

    public virtual ICollection<TblExamMark> TblExamMarks { get; set; } = new List<TblExamMark>();

    public virtual ICollection<TblFeeCollection> TblFeeCollections { get; set; } = new List<TblFeeCollection>();

    public virtual ICollection<TblReportCard> TblReportCards { get; set; } = new List<TblReportCard>();

    public virtual ICollection<TblStudentAttendance> TblStudentAttendances { get; set; } = new List<TblStudentAttendance>();

    public virtual ICollection<TblStudentDocument> TblStudentDocuments { get; set; } = new List<TblStudentDocument>();

    public virtual ICollection<TblStudentDue> TblStudentDues { get; set; } = new List<TblStudentDue>();

    public virtual ICollection<TblStudentExit> TblStudentExits { get; set; } = new List<TblStudentExit>();

    public virtual ICollection<TblStudentExtraCharge> TblStudentExtraCharges { get; set; } = new List<TblStudentExtraCharge>();

    public virtual ICollection<TblStudentFeeOverride> TblStudentFeeOverrides { get; set; } = new List<TblStudentFeeOverride>();

    public virtual ICollection<TblStudentMedical> TblStudentMedicals { get; set; } = new List<TblStudentMedical>();

    public virtual ICollection<TblStudentOptionalFee> TblStudentOptionalFees { get; set; } = new List<TblStudentOptionalFee>();

    public virtual ICollection<TblStudentParent> TblStudentParents { get; set; } = new List<TblStudentParent>();

    public virtual ICollection<TblStudentSession> TblStudentSessions { get; set; } = new List<TblStudentSession>();

    public virtual ICollection<TblTrnNotificationLog> TblTrnNotificationLogs { get; set; } = new List<TblTrnNotificationLog>();

    public virtual ICollection<TblTrnStudentAssignment> TblTrnStudentAssignments { get; set; } = new List<TblTrnStudentAssignment>();

    public virtual ICollection<TblTrnTripBoardingLog> TblTrnTripBoardingLogs { get; set; } = new List<TblTrnTripBoardingLog>();

    public virtual ICollection<UserMaster> UserMasters { get; set; } = new List<UserMaster>();
}
