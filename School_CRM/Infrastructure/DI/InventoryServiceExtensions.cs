using School_CRM.Services.Interface;
using School_CRM.Services.Inventory;
using School_CRM.Services.Inventory.Repository;

namespace School_CRM.Infrastructure.DI
{
    /// <summary>
    /// Registers every repository and service for the
    /// School Store / Inventory Management module.
    ///
    /// Usage in Program.cs:
    ///     builder.Services.AddInventoryServices();
    /// </summary>
    public static class InventoryServiceExtensions
    {
        public static IServiceCollection AddInventoryServices(this IServiceCollection services)
        {
            // ── Repositories ──────────────────────────────────────────────
            services.AddScoped<IInvCategoryRepository,        InvCategoryRepository>();
            services.AddScoped<IInvUnitRepository,            InvUnitRepository>();
            services.AddScoped<IInvSupplierRepository,        InvSupplierRepository>();
            services.AddScoped<IInvProductRepository,         InvProductRepository>();
            services.AddScoped<IInvPurchaseOrderRepository,   InvPurchaseOrderRepository>();
            services.AddScoped<IInvStockReceiptRepository,    InvStockReceiptRepository>();
            services.AddScoped<IInvSaleRepository,            InvSaleRepository>();
            services.AddScoped<IInvCreditLedgerRepository,    InvCreditLedgerRepository>();
            services.AddScoped<IInvStockAdjustmentRepository, InvStockAdjustmentRepository>();

            // ── Business Services ─────────────────────────────────────────
            services.AddScoped<IInvCategoryService,        InvCategoryService>();
            services.AddScoped<IInvUnitService,            InvUnitService>();
            services.AddScoped<IInvSupplierService,        InvSupplierService>();
            services.AddScoped<IInvProductService,         InvProductService>();
            services.AddScoped<IInvPurchaseOrderService,   InvPurchaseOrderService>();
            services.AddScoped<IInvStockReceiptService,    InvStockReceiptService>();
            services.AddScoped<IInvSaleService,            InvSaleService>();
            services.AddScoped<IInvStockAdjustmentService, InvStockAdjustmentService>();
            services.AddScoped<IInvDashboardService,       InvDashboardService>();
            services.AddScoped<IInvPersonService,          InvPersonService>();

            return services;
        }
    }
}
