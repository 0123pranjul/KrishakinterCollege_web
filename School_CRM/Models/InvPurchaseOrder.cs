using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvPurchaseOrder
{
    public int Poid { get; set; }

    public string Ponumber { get; set; } = null!;

    public int SupplierId { get; set; }

    public DateOnly OrderDate { get; set; }

    public DateOnly? ExpectedDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public string? Remarks { get; set; }

    public int CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public int? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public virtual ICollection<InvPurchaseOrderItem> InvPurchaseOrderItems { get; set; } = new List<InvPurchaseOrderItem>();

    public virtual ICollection<InvStockReceipt> InvStockReceipts { get; set; } = new List<InvStockReceipt>();

    public virtual InvSupplier Supplier { get; set; } = null!;
}
