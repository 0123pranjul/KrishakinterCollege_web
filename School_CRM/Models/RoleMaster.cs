using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class RoleMaster
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual ICollection<TblMenuPermission> TblMenuPermissions { get; set; } = new List<TblMenuPermission>();

    public virtual ICollection<UserRoleAssign> UserRoleAssigns { get; set; } = new List<UserRoleAssign>();
}
