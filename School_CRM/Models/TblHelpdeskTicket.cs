using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblHelpdeskTicket
{
    public int Id { get; set; }

    public string TicketNo { get; set; } = null!;

    public int CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Priority { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? AttachmentUrl { get; set; }

    public int RaisedBy { get; set; }

    public int? AssignedTo { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ResolvedDate { get; set; }

    public string? Remarks { get; set; }

    public virtual Employee? AssignedToNavigation { get; set; }

    public virtual TblHelpdeskCategory Category { get; set; } = null!;

    public virtual UserMaster RaisedByNavigation { get; set; } = null!;

    public virtual ICollection<TblHelpdeskReply> TblHelpdeskReplies { get; set; } = new List<TblHelpdeskReply>();
}
