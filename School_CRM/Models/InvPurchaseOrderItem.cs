using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvPurchaseOrderItem
{
    public int PoitemId { get; set; }

    public int Poid { get; set; }

    public int ProductId { get; set; }

    public int OrderQty { get; set; }

    public int ReceivedQty { get; set; }

    public decimal UnitCostPrice { get; set; }

    public decimal? TotalCost { get; set; }

    public string? Remarks { get; set; }

    public virtual ICollection<InvStockReceiptItem> InvStockReceiptItems { get; set; } = new List<InvStockReceiptItem>();

    public virtual InvPurchaseOrder Po { get; set; } = null!;

    public virtual InvProduct Product { get; set; } = null!;
}
