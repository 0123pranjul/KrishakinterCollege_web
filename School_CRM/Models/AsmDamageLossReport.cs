using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class AsmDamageLossReport
{
    public int ReportId { get; set; }

    public int UnitId { get; set; }

    public int? IssueId { get; set; }

    public string ReportType { get; set; } = null!;

    public int ReportedBy { get; set; }

    public DateOnly ReportDate { get; set; }

    public string Description { get; set; } = null!;

    public string? ResponsibleType { get; set; }

    public int? ResponsibleId { get; set; }

    public decimal EstimatedLoss { get; set; }

    public decimal FineImposed { get; set; }

    public bool IsFinePaid { get; set; }

    public DateOnly? FinePaidDate { get; set; }

    public string? ActionTaken { get; set; }

    public string Status { get; set; } = null!;

    public int? ResolvedBy { get; set; }

    public DateOnly? ResolvedDate { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual AsmIssueTransaction? Issue { get; set; }

    public virtual AsmAssetUnit Unit { get; set; } = null!;
}
