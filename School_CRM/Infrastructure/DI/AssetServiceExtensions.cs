using School_CRM.Services.Asset;
using School_CRM.Services.Asset.Repository;
using School_CRM.Services.Interface;

namespace School_CRM.Infrastructure.DI
{
    /// <summary>
    /// Registers every repository, service, and utility for the
    /// Asset Management module.
    ///
    /// Usage in Program.cs:
    ///     builder.Services.AddAssetServices();
    /// </summary>
    public static class AssetServiceExtensions
    {
        public static IServiceCollection AddAssetServices(this IServiceCollection services)
        {
            // ── Repositories ──────────────────────────────────────────────
            services.AddScoped<IAssetCategoryRepository,       AssetCategoryRepository>();
            services.AddScoped<IAssetSubCategoryRepository,    AssetSubCategoryRepository>();
            services.AddScoped<IAssetLocationRepository,       AssetLocationRepository>();
            services.AddScoped<IAssetVendorRepository,         AssetVendorRepository>();
            services.AddScoped<IAssetMasterRepository,         AssetMasterRepository>();
            services.AddScoped<IAssetUnitRepository,           AssetUnitRepository>();
            services.AddScoped<IAssetIssueRepository,          AssetIssueRepository>();
            services.AddScoped<IAssetLocationHistoryRepository, AssetLocationHistoryRepository>();
            services.AddScoped<IAssetMaintenanceRepository,    AssetMaintenanceRepository>();
            services.AddScoped<IAssetDamageReportRepository,   AssetDamageReportRepository>();
            services.AddScoped<IAssetDisposalRepository,       AssetDisposalRepository>();

            // ── Business Services ─────────────────────────────────────────
            services.AddScoped<IAssetCategoryService,    AssetCategoryService>();
            services.AddScoped<IAssetSubCategoryService, AssetSubCategoryService>();
            services.AddScoped<IAssetLocationService,    AssetLocationService>();
            services.AddScoped<IAssetVendorService,      AssetVendorService>();
            services.AddScoped<IAssetMasterService,      AssetMasterService>();
            services.AddScoped<IAssetIssueService,       AssetIssueService>();
            services.AddScoped<IAssetMaintenanceService, AssetMaintenanceService>();
            services.AddScoped<IAssetDamageReportService, AssetDamageReportService>();
            services.AddScoped<IAssetDisposalService,    AssetDisposalService>();
            services.AddScoped<IAssetDashboardService,   AssetDashboardService>();
            services.AddScoped<IAssetPersonService,      AssetPersonService>();

            // ── Utilities ─────────────────────────────────────────────────
            services.AddScoped<AssetQRCodeService>();

            return services;
        }
    }
}
