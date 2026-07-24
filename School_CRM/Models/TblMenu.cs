using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblMenu
{
    public int MenuId { get; set; }

    public int? ParentId { get; set; }

    public string MenuName { get; set; } = null!;

    public string? ControllerName { get; set; }

    public string? ActionName { get; set; }

    public string? Url { get; set; }

    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<TblMenu> InverseParent { get; set; } = new List<TblMenu>();

    public virtual TblMenu? Parent { get; set; }

    public virtual ICollection<TblMenuPermission> TblMenuPermissions { get; set; } = new List<TblMenuPermission>();
}
