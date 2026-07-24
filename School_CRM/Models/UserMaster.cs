using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class UserMaster
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool? IsActive { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public int? UpdatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public int? EmpId { get; set; }

    public int? StudentId { get; set; }

    public int? TeacherId { get; set; }

    public virtual Employee? Emp { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual TblStudent? Student { get; set; }

    public virtual ICollection<TblHelpdeskReply> TblHelpdeskReplies { get; set; } = new List<TblHelpdeskReply>();

    public virtual ICollection<TblHelpdeskTicket> TblHelpdeskTickets { get; set; } = new List<TblHelpdeskTicket>();

    public virtual TblTeacher? Teacher { get; set; }

    public virtual ICollection<UserOtp> UserOtps { get; set; } = new List<UserOtp>();

    public virtual ICollection<UserRoleAssign> UserRoleAssigns { get; set; } = new List<UserRoleAssign>();
}
