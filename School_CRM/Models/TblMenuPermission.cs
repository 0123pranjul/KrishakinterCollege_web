using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblMenuPermission
{
    public int PermissionId { get; set; }

    public int RoleId { get; set; }

    public int MenuId { get; set; }

    public bool CanRead { get; set; }

    public bool CanCreate { get; set; }

    public bool CanUpdate { get; set; }

    public bool CanDelete { get; set; }

    public virtual TblMenu Menu { get; set; } = null!;

    public virtual RoleMaster Role { get; set; } = null!;
}
