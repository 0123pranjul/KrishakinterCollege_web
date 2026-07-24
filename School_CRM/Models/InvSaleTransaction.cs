using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvSaleTransaction
{
    public int SaleId { get; set; }

    public string BillNumber { get; set; } = null!;

    public string BillType { get; set; } = null!;

    public string CustomerType { get; set; } = null!;

    public int? CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; }

    public decimal Gstamount { get; set; }

    public decimal TotalAmount { get; set; }

    public string PaymentMode { get; set; } = null!;

    public decimal AmountPaid { get; set; }

    public decimal? BalanceDue { get; set; }

    public bool IsPaid { get; set; }

    public DateOnly? DueDate { get; set; }

    public DateOnly SaleDate { get; set; }

    public int SoldBy { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<InvCreditLedger> InvCreditLedgers { get; set; } = new List<InvCreditLedger>();

    public virtual ICollection<InvSaleItem> InvSaleItems { get; set; } = new List<InvSaleItem>();
}
