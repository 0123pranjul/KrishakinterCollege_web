using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblPeriod
{
    public int PeriodId { get; set; }

    public string PeriodName { get; set; } = null!;

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int SequenceNo { get; set; }

    public bool? IsBrake { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblTimeTable> TblTimeTables { get; set; } = new List<TblTimeTable>();
}
