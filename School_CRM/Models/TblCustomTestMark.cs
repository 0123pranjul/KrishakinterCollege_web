using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblCustomTestMark
{
    public int Id { get; set; }

    public int TestId { get; set; }

    public int StudentId { get; set; }

    public decimal? MarksObtained { get; set; }

    public bool? IsAbsent { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual TblStudent Student { get; set; } = null!;

    public virtual TblCustomTest Test { get; set; } = null!;
}
