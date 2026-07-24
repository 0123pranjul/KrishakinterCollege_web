using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Inventory
{
    // ── Purchase Order ────────────────────────────────────────────────────────
    public class InvPurchaseOrderService : IInvPurchaseOrderService
    {
        private readonly IInvPurchaseOrderRepository _repo;
        public InvPurchaseOrderService(IInvPurchaseOrderRepository repo) => _repo = repo;

        public Task<List<InvPurchaseOrder>> GetAllAsync(int? supplierId = null, string? status = null) =>
            _repo.GetAllAsync(supplierId, status);

        public Task<InvPurchaseOrder?> GetByIdAsync(int id) => _repo.GetByIdWithItemsAsync(id);
        public Task<List<InvPurchaseOrder>> GetPendingAsync() => _repo.GetPendingAsync();

        public async Task<(bool Success, string Message, int POId)> CreateAsync(InvPurchaseOrderDto dto, int createdBy)
        {
            if (!dto.Items.Any()) return (false, "At least one item is required.", 0);

            var poNumber = await _repo.GeneratePONumberAsync();

            var po = new InvPurchaseOrder
            {
                Ponumber    = poNumber,
                SupplierId  = dto.SupplierId,
                OrderDate   = dto.OrderDate,
                ExpectedDate = dto.ExpectedDate,
                Status      = "Draft",
                TotalAmount = 0,
                Remarks     = dto.Remarks,
                CreatedBy   = createdBy,
                CreatedAt   = DateTime.Now
            };

            var items = dto.Items.Select(i => new InvPurchaseOrderItem
            {
                ProductId     = i.ProductId,
                OrderQty      = i.OrderQty,
                ReceivedQty   = 0,
                UnitCostPrice = i.UnitCostPrice,
                Remarks       = i.Remarks
            }).ToList();

            await _repo.CreateAsync(po, items);
            return (true, $"Purchase Order {poNumber} created.", po.Poid);
        }

        public async Task<(bool Success, string Message)> SendToSupplierAsync(int id, int userId)
        {
            var ok = await _repo.UpdateStatusAsync(id, "Sent", userId);
            return ok ? (true, "PO sent to supplier.") : (false, "PO not found.");
        }

        public async Task<(bool Success, string Message)> CancelAsync(int id)
        {
            var ok = await _repo.UpdateStatusAsync(id, "Cancelled");
            return ok ? (true, "PO cancelled.") : (false, "PO not found.");
        }
    }

    // ── Stock Receipt / GRN ───────────────────────────────────────────────────
    public class InvStockReceiptService : IInvStockReceiptService
    {
        private readonly IInvStockReceiptRepository  _receiptRepo;
        private readonly IInvPurchaseOrderRepository _poRepo;
        private readonly IInvProductRepository       _productRepo;
        private readonly LibmanagementContext         _db;

        public InvStockReceiptService(
            IInvStockReceiptRepository receiptRepo,
            IInvPurchaseOrderRepository poRepo,
            IInvProductRepository productRepo,
            LibmanagementContext db)
        {
            _receiptRepo = receiptRepo;
            _poRepo      = poRepo;
            _productRepo = productRepo;
            _db          = db;
        }

        public Task<List<InvStockReceipt>> GetAllAsync(int? supplierId = null) =>
            _receiptRepo.GetAllAsync(supplierId);

        public Task<InvStockReceipt?> GetByIdAsync(int id) =>
            _receiptRepo.GetByIdWithItemsAsync(id);

        public async Task<List<InvPOItemDto>> GetPOItemsAsync(int poId)
        {
            var po = await _poRepo.GetByIdWithItemsAsync(poId);
            if (po == null) return new();

            return po.InvPurchaseOrderItems.Select(i => new InvPOItemDto
            {
                POItemId      = i.PoitemId,
                POId          = i.Poid,
                ProductId     = i.ProductId,
                ProductName   = i.Product.ProductName,
                UnitShort     = i.Product.Unit.UnitShort,
                OrderQty      = i.OrderQty,
                ReceivedQty   = i.ReceivedQty,
                UnitCostPrice = i.UnitCostPrice
            }).ToList();
        }

        public async Task<(bool Success, string Message, int ReceiptId)> CreateAsync(
            InvStockReceiptDto dto, int receivedBy)
        {
            if (!dto.Items.Any()) return (false, "At least one item is required.", 0);

            var grnNumber = await _receiptRepo.GenerateGRNNumberAsync();

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                var receipt = new InvStockReceipt
                {
                    Grnnumber     = grnNumber,
                    Poid          = dto.POId,
                    SupplierId    = dto.SupplierId,
                    ReceiptDate   = dto.ReceiptDate,
                    InvoiceNo     = dto.InvoiceNo,
                    InvoiceDate   = dto.InvoiceDate,
                    InvoiceAmount = dto.InvoiceAmount,
                    TotalAmount   = 0,
                    Status        = "Received",
                    Remarks       = dto.Remarks,
                    ReceivedBy    = receivedBy,
                    CreatedAt     = DateTime.Now
                };

                var items = dto.Items.Select(i => new InvStockReceiptItem
                {
                    ProductId     = i.ProductId,
                    PoitemId      = i.POItemId > 0 ? i.POItemId : null,
                    ReceivedQty   = i.ReceivedQty,
                    UnitCostPrice = i.UnitCostPrice,
                    BatchNo       = i.BatchNo,
                    ExpiryDate    = i.ExpiryDate,
                    Remarks       = i.Remarks
                }).ToList();

                await _receiptRepo.CreateAsync(receipt, items);

                // Update stock for each item
                foreach (var item in dto.Items)
                {
                    await _productRepo.UpdateStockAsync(item.ProductId, item.ReceivedQty);

                    // Update PO item received qty
                    if (item.POItemId > 0)
                    {
                        var poItem = await _db.InvPurchaseOrderItems.FindAsync(item.POItemId);
                        if (poItem != null)
                        {
                            poItem.ReceivedQty += item.ReceivedQty;
                        }
                    }
                }

                // Update PO status if linked
                if (dto.POId.HasValue)
                {
                    var po = await _poRepo.GetByIdWithItemsAsync(dto.POId.Value);
                    if (po != null)
                    {
                        bool allReceived = po.InvPurchaseOrderItems
                            .All(i => i.ReceivedQty >= i.OrderQty);
                        await _poRepo.UpdateStatusAsync(
                            dto.POId.Value,
                            allReceived ? "Received" : "PartialReceived");
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();

                return (true, $"GRN {grnNumber} created. Stock updated.", receipt.ReceiptId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, $"GRN failed: {ex.Message}", 0);
            }
        }
    }

    // ── Sale / Issue ──────────────────────────────────────────────────────────
    public class InvSaleService : IInvSaleService
    {
        private readonly IInvSaleRepository         _saleRepo;
        private readonly IInvCreditLedgerRepository _creditRepo;
        private readonly IInvProductRepository      _productRepo;
        private readonly LibmanagementContext        _db;

        public InvSaleService(
            IInvSaleRepository saleRepo,
            IInvCreditLedgerRepository creditRepo,
            IInvProductRepository productRepo,
            LibmanagementContext db)
        {
            _saleRepo    = saleRepo;
            _creditRepo  = creditRepo;
            _productRepo = productRepo;
            _db          = db;
        }

        public Task<(List<InvSaleTransaction> Items, int Total)> GetAllAsync(InvSaleFilterDto filter) =>
            _saleRepo.GetAllAsync(filter);

        public Task<InvSaleTransaction?> GetByIdAsync(int id) =>
            _saleRepo.GetByIdWithItemsAsync(id);

        public Task<List<InvSaleTransaction>> GetUnpaidByCustomerAsync(string type, int id) =>
            _saleRepo.GetUnpaidByCustomerAsync(type, id);

        public async Task<(bool Success, string Message, int SaleId)> CreateAsync(InvSaleDto dto, int soldBy)
        {
            if (!dto.Items.Any()) return (false, "At least one item is required.", 0);

            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Stock validation
                foreach (var item in dto.Items)
                {
                    var product = await _db.InvProducts.FindAsync(item.ProductId);
                    if (product == null)
                        throw new Exception($"Product #{item.ProductId} not found.");
                    if (product.CurrentStock < item.Qty)
                        throw new Exception($"'{product.ProductName}': insufficient stock. Available: {product.CurrentStock}");
                }

                var billNumber = await _saleRepo.GenerateBillNumberAsync();
                bool isCredit  = dto.PaymentMode == "Credit";
                bool isFree    = dto.PaymentMode == "Free" || dto.BillType == "Issue";

                var sale = new InvSaleTransaction
                {
                    BillNumber     = billNumber,
                    BillType       = dto.BillType,
                    CustomerType   = dto.CustomerType,
                    CustomerId     = dto.CustomerId,
                    CustomerName   = dto.CustomerName,
                    SubTotal       = dto.SubTotal,
                    DiscountAmount = dto.DiscountAmount,
                    Gstamount      = dto.GSTAmount,
                    TotalAmount    = dto.TotalAmount,
                    PaymentMode    = dto.PaymentMode,
                    AmountPaid     = isFree ? 0 : (isCredit ? 0 : dto.AmountPaid),
                    IsPaid         = !isCredit && !isFree,
                    DueDate        = isCredit ? DateOnly.FromDateTime(DateTime.Today.AddDays(30)) : null,
                    SaleDate       = dto.SaleDate,
                    SoldBy         = soldBy,
                    Remarks        = dto.Remarks,
                    CreatedAt      = DateTime.Now
                };

                var items = dto.Items.Select(i =>
                {
                    decimal lineTotal = (i.Qty * i.UnitSellingPrice)
                        * (1 - i.DiscountPercent / 100)
                        * (1 + i.GSTPercent / 100);

                    return new InvSaleItem
                    {
                        ProductId        = i.ProductId,
                        Qty              = i.Qty,
                        UnitSellingPrice = i.UnitSellingPrice,
                        DiscountPercent  = i.DiscountPercent,
                        Gstpercent       = i.GSTPercent,
                        LineTotal        = Math.Round(lineTotal, 2),
                        Remarks          = i.Remarks
                    };
                }).ToList();

                await _saleRepo.CreateAsync(sale, items);

                // Deduct stock
                foreach (var item in dto.Items)
                    await _productRepo.UpdateStockAsync(item.ProductId, -item.Qty);

                // Credit ledger entry
                if (isCredit && dto.CustomerId.HasValue)
                {
                    await _creditRepo.CreateAsync(new InvCreditLedger
                    {
                        CustomerType    = dto.CustomerType,
                        CustomerId      = dto.CustomerId.Value,
                        SaleId          = sale.SaleId,
                        TransactionType = "Debit",
                        Amount          = dto.TotalAmount,
                        Description     = $"Credit sale - {billNumber}",
                        TransactionDate = dto.SaleDate,
                        CreatedAt       = DateTime.Now
                    });
                }

                await tx.CommitAsync();
                return (true, $"Bill {billNumber} created successfully.", sale.SaleId);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, ex.Message, 0);
            }
        }

        public async Task<InvCreditPaymentDto?> GetCreditDetailsAsync(string type, int id)
        {
            var balance  = await _creditRepo.GetBalanceAsync(type, id);
            var unpaid   = await _saleRepo.GetUnpaidByCustomerAsync(type, id);

            return new InvCreditPaymentDto
            {
                CustomerType = type,
                CustomerId   = id,
                TotalDue     = balance,
                UnpaidBills  = unpaid.Select(x => new InvUnpaidBillDto
                {
                    SaleId      = x.SaleId,
                    BillNumber  = x.BillNumber,
                    SaleDate    = x.SaleDate,
                    TotalAmount = x.TotalAmount,
                    AmountPaid  = x.AmountPaid,
                    BalanceDue  = x.BalanceDue ?? 0
                }).ToList()
            };
        }

        public async Task<(bool Success, string Message)> CollectPaymentAsync(
            InvCreditPaymentDto dto, int receivedBy)
        {
            using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // Insert credit entry
                await _creditRepo.CreateAsync(new InvCreditLedger
                {
                    CustomerType    = dto.CustomerType,
                    CustomerId      = dto.CustomerId,
                    TransactionType = "Credit",
                    Amount          = dto.AmountPaid,
                    Description     = "Payment received",
                    TransactionDate = dto.PaymentDate,
                    ReceivedBy      = receivedBy,
                    CreatedAt       = DateTime.Now
                });

                // FIFO: close oldest unpaid bills first
                decimal remaining = dto.AmountPaid;
                foreach (var bill in dto.UnpaidBills.OrderBy(x => x.SaleDate))
                {
                    if (remaining <= 0) break;
                    if (remaining >= bill.BalanceDue)
                    {
                        await _saleRepo.MarkPaidAsync(bill.SaleId);
                        remaining -= bill.BalanceDue;
                    }
                    else
                    {
                        // Partial payment — update AmountPaid
                        var sale = await _db.InvSaleTransactions.FindAsync(bill.SaleId);
                        if (sale != null)
                        {
                            sale.AmountPaid += remaining;
                            remaining = 0;
                        }
                    }
                }

                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return (true, $"Payment of ₹{dto.AmountPaid:F2} collected.");
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return (false, ex.Message);
            }
        }
    }

    // ── Stock Adjustment ──────────────────────────────────────────────────────
    public class InvStockAdjustmentService : IInvStockAdjustmentService
    {
        private readonly IInvStockAdjustmentRepository _repo;
        private readonly IInvProductRepository         _productRepo;

        public InvStockAdjustmentService(
            IInvStockAdjustmentRepository repo,
            IInvProductRepository productRepo)
        {
            _repo        = repo;
            _productRepo = productRepo;
        }

        public async Task<(bool Success, string Message)> AdjustAsync(InvStockAdjustmentDto dto, int userId)
        {
            var product = await _productRepo.GetByIdAsync(dto.ProductId);
            if (product == null) return (false, "Product not found.");

            int newStock = product.CurrentStock + dto.AdjustedQty;
            if (newStock < 0)
                return (false, $"Adjustment would result in negative stock ({newStock}). Not allowed.");

            await _repo.CreateAsync(new InvStockAdjustment
            {
                ProductId      = dto.ProductId,
                AdjustmentType = dto.AdjustmentType,
                QuantityBefore = product.CurrentStock,
                AdjustedQty    = dto.AdjustedQty,
                Reason         = dto.Reason,
                Remarks        = dto.Remarks,
                AdjustedBy     = userId,
                AdjustedAt     = DateTime.Now
            });

            await _productRepo.UpdateStockAsync(dto.ProductId, dto.AdjustedQty);
            return (true, $"Stock adjusted. New stock: {newStock}");
        }

        public Task<List<InvStockAdjustment>> GetByProductAsync(int productId) =>
            _repo.GetByProductAsync(productId);
    }
}
