using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblStudentAttendance
{
    public int AttendanceId { get; set; }

    public int StudentId { get; set; }

    public int SessionId { get; set; }

    public int ClassId { get; set; }

    public int SectionId { get; set; }

    public DateOnly AttendanceDate { get; set; }

    public string Status { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblClass Class { get; set; } = null!;

    public virtual TblSection Section { get; set; } = null!;

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;
}
