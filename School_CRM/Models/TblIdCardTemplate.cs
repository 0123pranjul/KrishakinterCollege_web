using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblIdCardTemplate
{
    public int TemplateId { get; set; }

    public string TemplateName { get; set; } = null!;

    public string Orientation { get; set; } = null!;

    public string? SchoolName { get; set; }

    public string? SchoolAddress { get; set; }

    public string? SchoolContact { get; set; }

    public string? ThemeColor { get; set; }

    public string? BackgroundFrontPath { get; set; }

    public string? BackgroundBackPath { get; set; }

    public string? SchoolLogoPath { get; set; }

    public string? PrincipalSignaturePath { get; set; }

    public string? FieldsConfigJson { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }
}
