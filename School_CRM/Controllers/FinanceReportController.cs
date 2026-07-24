using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace School_CRM.Controllers
{
    public class FinanceReportController : Controller
    {
        private readonly LibmanagementContext _context;

        public FinanceReportController(LibmanagementContext context)
        {
            _context = context;
        }

        private bool IsAdmin => HttpContext.Request.Cookies["IsAdmin"] == "true";

        // GET: FinanceReport/Income
        public IActionResult Income()
        {
            if (!IsAdmin)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: FinanceReport/Expense
        public IActionResult Expense()
        {
            if (!IsAdmin)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // GET: FinanceReport/GetIncomeData
        [HttpGet]
        public async Task<IActionResult> GetIncomeData(DateTime? startDate, DateTime? endDate)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            var query = _context.TblFeeCollections
                .Include(f => f.Student)
                .Include(f => f.Session)
                .Where(f => f.IsActive == true);

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(f => f.PaymentDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(f => f.PaymentDate <= end);
            }

            var collections = await query.ToListAsync();
            var userIds = collections.Select(c => c.CreatedBy).Distinct().ToList();

            var users = await _context.UserMasters
                .Where(u => userIds.Contains(u.UserId))
                .ToDictionaryAsync(u => u.UserId, u => u.Username);

            var data = collections.Select(f => new
            {
                f.FeeCollectionId,
                StudentName = f.Student != null ? f.Student.StudentName : "-",
                RollNo = f.Student != null ? f.Student.RollNo : "-",
                SessionName = f.Session != null ? f.Session.SessionName : "-",
                MonthYear = f.Month != null && f.Year != null
                    ? System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(f.Month.Value) + " " + f.Year
                    : "-",
                TotalAmount = f.TotalAmount ?? 0m,
                PaidAmount = f.PaidAmount ?? 0m,
                DiscountAmount = f.DiscountAmount ?? 0m,
                FineAmount = f.FineAmount ?? 0m,
                DueAmount = (f.TotalAmount ?? 0m) - (f.PaidAmount ?? 0m),
                PaymentMode = f.PaymentMode ?? "-",
                PaymentDate = f.PaymentDate.HasValue ? f.PaymentDate.Value.ToString("dd-MM-yyyy") : "-",
                CollectedBy = (f.CreatedBy.HasValue && users.ContainsKey(f.CreatedBy.Value)) ? users[f.CreatedBy.Value] : "System"
            }).OrderByDescending(x => x.FeeCollectionId).ToList();

            return Json(new { success = true, data });
        }

        // GET: FinanceReport/GetExpenseData
        [HttpGet]
        public async Task<IActionResult> GetExpenseData(DateTime? startDate, DateTime? endDate)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            // 1. Get Salaries
            var salaryQuery = _context.SalaryMasters
                .Include(s => s.Employee)
                .AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                salaryQuery = salaryQuery.Where(s => s.GeneratedDate >= start);
            }
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                salaryQuery = salaryQuery.Where(s => s.GeneratedDate <= end);
            }

            var salaries = await salaryQuery.ToListAsync();

            var salaryData = salaries.Select(s => new
            {
                Type = "Salary",
                EmployeeName = s.Employee?.Name ?? "-",
                EmployeeCode = s.Employee?.EmployeeCode ?? "-",
                Designation = s.Employee?.Designation ?? "-",
                MonthYear = s.MonthYear,
                Amount = s.NetSalary ?? 0m,
                GrossAmount = s.GrossSalary ?? 0m,
                Deductions = s.Deductions ?? 0m,
                ExpenseDate = s.GeneratedDate.HasValue ? s.GeneratedDate.Value.ToString("dd-MM-yyyy") : "-",
                Status = s.Status,
                Remarks = $"Salary for {s.MonthYear}"
            }).ToList();

            // 2. Get Advances
            var advanceQuery = _context.EmployeeAdvances
                .Include(a => a.Employee)
                .Where(a => a.Status != "Deleted") // Assuming we show pending/deducted both as expense given
                .AsQueryable();

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                advanceQuery = advanceQuery.Where(a => a.AdvanceDate >= start);
            }
            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                advanceQuery = advanceQuery.Where(a => a.AdvanceDate <= end);
            }

            var advances = await advanceQuery.ToListAsync();

            var advanceData = advances.Select(a => new
            {
                Type = "Advance",
                EmployeeName = a.Employee?.Name ?? "-",
                EmployeeCode = a.Employee?.EmployeeCode ?? "-",
                Designation = a.Employee?.Designation ?? "-",
                MonthYear = a.DeductFromMonth ?? "-",
                Amount = a.Amount,
                GrossAmount = a.Amount,
                Deductions = 0m,
                ExpenseDate = a.AdvanceDate.HasValue ? a.AdvanceDate.Value.ToString("dd-MM-yyyy") : "-",
                Status = a.Status ?? "-",
                Remarks = $"Advance: {a.Reason}"
            }).ToList();

            // Combine both
            var combinedData = salaryData.Concat(advanceData).OrderByDescending(x => x.ExpenseDate).ToList();

            return Json(new { success = true, data = combinedData });
        }
    }
}
