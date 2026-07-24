using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class DocBuilderImage
{
    public int ImageId { get; set; }

    public int? DocumentId { get; set; }

    public string FileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long FileSize { get; set; }

    public string MimeType { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual DocBuilderDocument? Document { get; set; }
}
