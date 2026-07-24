using System;
using System.Collections.Generic;

namespace School_CRM.Models;

public partial class InvCreditLedger
{
    public int LedgerId { get; set; }

    public string CustomerType { get; set; } = null!;

    public int CustomerId { get; set; }

    public int? SaleId { get; set; }

    public string TransactionType { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Description { get; set; } = null!;

    public DateOnly TransactionDate { get; set; }

    public int? ReceivedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual InvSaleTransaction? Sale { get; set; }
}
