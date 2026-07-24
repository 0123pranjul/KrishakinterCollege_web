using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Services.Repository
{
    public class FinePaymentRepository : IFinePaymentRepository
    {
        private readonly LibmanagementContext _context;

        public FinePaymentRepository(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalPaidAsync(int issueId)
        {
            return await _context.LibFinePayments
                .Where(x => x.IssueId == issueId)
                .SumAsync(x => (decimal?)x.AmountPaid) ?? 0;
        }

        public async Task<List<LibFinePayment>> GetPaymentsByIssueAsync(int issueId)
        {
            return await _context.LibFinePayments
                .Where(x => x.IssueId == issueId)
                .OrderByDescending(x => x.PaymentDate)
                .ToListAsync();
        }

        public async Task<LibFinePayment> CreateAsync(LibFinePayment payment)
        {
            _context.LibFinePayments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<decimal> GetTodayCollectionAsync()
        {
            var today = DateTime.Today;
            return await _context.LibFinePayments
                .Where(x => x.PaymentDate.Date == today)
                .SumAsync(x => (decimal?)x.AmountPaid) ?? 0;
        }

        public async Task<List<MonthlyFineDto>> GetMonthlyCollectionAsync(int months)
        {
            var fromDate = DateTime.Today.AddMonths(-months);

            var data = await _context.LibFinePayments
                .Where(x => x.PaymentDate >= fromDate)
                .GroupBy(x => new { x.PaymentDate.Year, x.PaymentDate.Month })
                .Select(g => new
                {
                    Year  = g.Key.Year,
                    Month = g.Key.Month,
                    Total = g.Sum(x => x.AmountPaid)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return data.Select(x => new MonthlyFineDto
            {
                Month     = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"),
                TotalFine = x.Total
            }).ToList();
        }
    }
}
