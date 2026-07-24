using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class DocBuilderDocument
{
    public int DocumentId { get; set; }

    public string DocumentName { get; set; } = null!;

    public string DocumentType { get; set; } = null!;

    public int? TemplateId { get; set; }

    public string ComponentsJson { get; set; } = null!;

    public string? PrintSettingsJson { get; set; }

    public string Status { get; set; } = null!;

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DocBuilderImage> DocBuilderImages { get; set; } = new List<DocBuilderImage>();

    public virtual ICollection<DocBuilderQuestion> DocBuilderQuestions { get; set; } = new List<DocBuilderQuestion>();

    public virtual DocBuilderTemplate? Template { get; set; }
}
