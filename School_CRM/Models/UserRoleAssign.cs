using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class UserRoleAssign
{
    public int AssignmentId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public virtual RoleMaster Role { get; set; } = null!;

    public virtual UserMaster User { get; set; } = null!;
}
