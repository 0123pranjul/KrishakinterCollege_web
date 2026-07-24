using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblTrnSetting
{
    public int Id { get; set; }

    public string SettingKey { get; set; } = null!;

    public string SettingValue { get; set; } = null!;

    public string? Description { get; set; }
}
