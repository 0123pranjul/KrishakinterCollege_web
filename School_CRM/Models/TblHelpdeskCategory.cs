using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblHelpdeskCategory
{
    public int Id { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<TblHelpdeskTicket> TblHelpdeskTickets { get; set; } = new List<TblHelpdeskTicket>();
}
