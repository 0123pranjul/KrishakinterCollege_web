using School_CRM.Services;
using School_CRM.Services.Interface;
using School_CRM.Services.Repository;

namespace School_CRM.Infrastructure.DI
{
    /// <summary>
    /// Extension method that registers every repository, business service,
    /// and utility belonging to the Library Management module.
    ///
    /// Usage in Program.cs:
    ///     builder.Services.AddLibraryServices();
    /// </summary>
    public static class LibraryServiceExtensions
    {
        public static IServiceCollection AddLibraryServices(
            this IServiceCollection services)
        {
            // ── Repositories (Data Access Layer) ──────────────────────────
            services.AddScoped<IBookCategoryRepository,     BookCategoryRepository>();
            services.AddScoped<IBookRepository,             BookRepository>();
            services.AddScoped<IFinePolicyRepository,       FinePolicyRepository>();
            services.AddScoped<IIssueTransactionRepository, IssueTransactionRepository>();
            services.AddScoped<IMemberBlockRepository,      MemberBlockRepository>();
            services.AddScoped<IFinePaymentRepository,      FinePaymentRepository>();
            services.AddScoped<ILibSettingsRepository,      LibSettingsRepository>();

            // ── Business Services (Application Layer) ─────────────────────
            services.AddScoped<IBookCategoryService,    BookCategoryService>();
            services.AddScoped<IBookService,            BookService>();
            services.AddScoped<IFinePolicyService,      FinePolicyService>();
            services.AddScoped<IIssueService,           IssueService>();
            services.AddScoped<IFinePaymentService,     FinePaymentService>();
            services.AddScoped<ILibraryDashboardService, LibraryDashboardService>();
            services.AddScoped<ILibraryMemberService,   LibraryMemberService>();

            // ── Utilities ─────────────────────────────────────────────────
            services.AddScoped<QRCodeService>();

            return services;
        }
    }
}
