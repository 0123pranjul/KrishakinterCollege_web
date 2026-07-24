using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvSupplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? Gstno { get; set; }

    public decimal OpeningBalance { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public virtual ICollection<InvPurchaseOrder> InvPurchaseOrders { get; set; } = new List<InvPurchaseOrder>();

    public virtual ICollection<InvStockReceipt> InvStockReceipts { get; set; } = new List<InvStockReceipt>();
}
