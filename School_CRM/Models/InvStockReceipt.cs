using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvStockReceipt
{
    public int ReceiptId { get; set; }

    public string Grnnumber { get; set; } = null!;

    public int? Poid { get; set; }

    public int SupplierId { get; set; }

    public DateOnly ReceiptDate { get; set; }

    public string? InvoiceNo { get; set; }

    public DateOnly? InvoiceDate { get; set; }

    public decimal InvoiceAmount { get; set; }

    public decimal TotalAmount { get; set; }

    public string Status { get; set; } = null!;

    public string? Remarks { get; set; }

    public int ReceivedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InvStockReceiptItem> InvStockReceiptItems { get; set; } = new List<InvStockReceiptItem>();

    public virtual InvPurchaseOrder? Po { get; set; }

    public virtual InvSupplier Supplier { get; set; } = null!;
}
