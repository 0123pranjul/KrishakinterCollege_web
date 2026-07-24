using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblContactQuery
{
    public int QueryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Email { get; set; }

    public string Mobile { get; set; } = null!;

    public string? Subject { get; set; }

    public string Message { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }
}
