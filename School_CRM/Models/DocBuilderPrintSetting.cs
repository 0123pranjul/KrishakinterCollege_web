using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class DocBuilderPrintSetting
{
    public int SettingId { get; set; }

    public string SettingName { get; set; } = null!;

    public string PageSize { get; set; } = null!;

    public string Orientation { get; set; } = null!;

    public decimal MarginTop { get; set; }

    public decimal MarginBottom { get; set; }

    public decimal MarginLeft { get; set; }

    public decimal MarginRight { get; set; }

    public bool ShowHeader { get; set; }

    public string? HeaderText { get; set; }

    public bool ShowFooter { get; set; }

    public string? FooterText { get; set; }

    public bool ShowPageNumbers { get; set; }

    public string? WatermarkText { get; set; }

    public decimal? WatermarkOpacity { get; set; }

    public bool IsActive { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }
}
