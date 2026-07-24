using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class CommCircular
{
    public int CircularId { get; set; }

    public string CircularNo { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly CircularDate { get; set; }

    public string FilePath { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public int? FileSizeKb { get; set; }

    public string TargetType { get; set; } = null!;

    public int? TargetClassId { get; set; }

    public int? TargetSectionId { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
