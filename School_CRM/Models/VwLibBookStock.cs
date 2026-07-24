using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class VwLibBookStock
{
    public int BookId { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string CategoryName { get; set; } = null!;

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public int? IssuedCopies { get; set; }

    public string? ShelfLocation { get; set; }
}
