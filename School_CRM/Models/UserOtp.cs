using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class UserOtp
{
    public int OtpId { get; set; }

    public int UserId { get; set; }

    public string OtpCode { get; set; } = null!;

    public string VerificationToken { get; set; } = null!;

    public DateTime CreatedDateTime { get; set; }

    public DateTime ExpiryDateTime { get; set; }

    public bool IsVerified { get; set; }

    public string? IpAddress { get; set; }

    public virtual UserMaster User { get; set; } = null!;
}
