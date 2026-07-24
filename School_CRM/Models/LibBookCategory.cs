using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibBookCategory
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<LibBook> LibBooks { get; set; } = new List<LibBook>();
}
