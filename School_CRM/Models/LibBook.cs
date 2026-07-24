using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibBook
{
    public int BookId { get; set; }

    public string? Isbn { get; set; }

    public string Title { get; set; } = null!;

    public string Author { get; set; } = null!;

    public string? Publisher { get; set; }

    public short? PublishedYear { get; set; }

    public int CategoryId { get; set; }

    public string? Edition { get; set; }

    public string Language { get; set; } = null!;

    public string? ShelfLocation { get; set; }

    public string? Description { get; set; }

    public decimal BookPrice { get; set; }

    public int TotalCopies { get; set; }

    public int AvailableCopies { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual LibBookCategory Category { get; set; } = null!;

    public virtual ICollection<LibBookCopy> LibBookCopies { get; set; } = new List<LibBookCopy>();
}
