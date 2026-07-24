using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class TblHelpdeskReply
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public string ReplyMessage { get; set; } = null!;

    public string? AttachmentUrl { get; set; }

    public int ReplyBy { get; set; }

    public bool IsAdminReply { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual UserMaster ReplyByNavigation { get; set; } = null!;

    public virtual TblHelpdeskTicket Ticket { get; set; } = null!;
}
