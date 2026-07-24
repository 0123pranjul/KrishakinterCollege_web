using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvProduct
{
    public int ProductId { get; set; }

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public int CategoryId { get; set; }

    public int UnitId { get; set; }

    public decimal CostPrice { get; set; }

    public decimal SellingPrice { get; set; }

    public int CurrentStock { get; set; }

    public int ReorderLevel { get; set; }

    public int? MaxStockLevel { get; set; }

    public string? Description { get; set; }

    public string? Hsncode { get; set; }

    public decimal Gstpercent { get; set; }

    public string? ProductImagePath { get; set; }

    public string? Barcode { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public int CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedBy { get; set; }

    public virtual InvCategory Category { get; set; } = null!;

    public virtual ICollection<InvPurchaseOrderItem> InvPurchaseOrderItems { get; set; } = new List<InvPurchaseOrderItem>();

    public virtual ICollection<InvSaleItem> InvSaleItems { get; set; } = new List<InvSaleItem>();

    public virtual ICollection<InvStockAdjustment> InvStockAdjustments { get; set; } = new List<InvStockAdjustment>();

    public virtual ICollection<InvStockReceiptItem> InvStockReceiptItems { get; set; } = new List<InvStockReceiptItem>();

    public virtual InvUnit Unit { get; set; } = null!;
}
