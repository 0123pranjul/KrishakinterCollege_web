using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvStockReceiptItem
{
    public int ReceiptItemId { get; set; }

    public int ReceiptId { get; set; }

    public int ProductId { get; set; }

    public int? PoitemId { get; set; }

    public int ReceivedQty { get; set; }

    public decimal UnitCostPrice { get; set; }

    public string? BatchNo { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public string? Remarks { get; set; }

    public virtual InvPurchaseOrderItem? Poitem { get; set; }

    public virtual InvProduct Product { get; set; } = null!;

    public virtual InvStockReceipt Receipt { get; set; } = null!;
}
