using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnStudentAssignment
{
    public int Id { get; set; }

    public int StudentId { get; set; }

    public int RouteId { get; set; }

    public int StopId { get; set; }

    public int SessionId { get; set; }

    public string AssignmentStatus { get; set; } = null!;

    public int? OptionalFeeId { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblTrnRoute Route { get; set; } = null!;

    public virtual TblAcademicSession Session { get; set; } = null!;

    public virtual TblTrnRouteStop Stop { get; set; } = null!;

    public virtual TblStudent Student { get; set; } = null!;
}
