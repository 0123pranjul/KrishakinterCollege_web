using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class LibSetting
{
    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime UpdatedAt { get; set; }

    public int UpdatedBy { get; set; }
}
