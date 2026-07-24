using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvSaleItem
{
    public int SaleItemId { get; set; }

    public int SaleId { get; set; }

    public int ProductId { get; set; }

    public int Qty { get; set; }

    public decimal UnitSellingPrice { get; set; }

    public decimal DiscountPercent { get; set; }

    public decimal Gstpercent { get; set; }

    public decimal LineTotal { get; set; }

    public string? Remarks { get; set; }

    public virtual InvProduct Product { get; set; } = null!;

    public virtual InvSaleTransaction Sale { get; set; } = null!;
}
