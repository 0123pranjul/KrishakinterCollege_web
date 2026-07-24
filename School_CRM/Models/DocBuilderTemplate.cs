using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class DocBuilderTemplate
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string TemplateType { get; set; } = null!;

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public string ComponentsJson { get; set; } = null!;

    public string? PrintSettingsJson { get; set; }

    public bool IsSystem { get; set; }

    public bool IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<DocBuilderDocument> DocBuilderDocuments { get; set; } = new List<DocBuilderDocument>();
}
