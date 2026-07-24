using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ReportController : Controller
    {
        private readonly LibmanagementContext _context;

        public ReportController(LibmanagementContext context)
        {
            _context = context;
        }

        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";

        private int CurrentEmployeeId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"],
                out var id) ? id : 0;

        public IActionResult Index()
        {
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        // ── Monthly Summary ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetMonthlySummary(
            string monthYear)
        {
            var query = _context.SalaryMasters
                .Include(s => s.Employee)
                .Where(s => s.MonthYear == monthYear);

            if (!IsAdmin)
                query = query.Where(s =>
                    s.EmployeeId == CurrentEmployeeId);

            var data = await query.ToListAsync();

            if (!data.Any())
                return Json(new
                {
                    success = false,
                    message = "No salary data found!"
                });

            var summary = new
            {
                monthYear,
                totalEmployees = data.Count,
                totalGross = data.Sum(s => s.GrossSalary ?? 0),
                totalDeductions = data.Sum(s => s.Deductions ?? 0),
                totalNet = data.Sum(s => s.NetSalary ?? 0),
                totalOT = data.Sum(s => s.OvertimeAmount ?? 0),
                paidCount = data.Count(s => s.Status == "Paid"),
                pendingCount = data.Count(s => s.Status != "Paid"),
                paidAmount = data.Where(s => s.Status == "Paid")
                                      .Sum(s => s.NetSalary ?? 0),
                pendingAmount = data.Where(s => s.Status != "Paid")
                                      .Sum(s => s.NetSalary ?? 0),
                details = data.Select(s => new {
                    s.Id,
                    employeeCode = s.Employee?.EmployeeCode,
                    employeeName = s.Employee?.Name,
                    designation = s.Employee?.Designation,
                    department = s.Employee?.Department,
                    s.PayableDays,
                    s.PresentDays,
                    s.HolidayDays,
                    s.LeaveDays,
                    s.LwpDays,
                    s.OvertimeHours,
                    s.BasicSalary,
                    s.OvertimeAmount,
                    s.GrossSalary,
                    s.Deductions,
                    s.NetSalary,
                    s.Status,
                    generatedDate = s.GeneratedDate?.ToString(
                        "dd-MM-yyyy")
                }).ToList()
            };

            return Json(new { success = true, data = summary });
        }

        // ── Year-wise Summary ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetYearlySummary(int year)
        {
            var empId = CurrentEmployeeId;

            var query = _context.SalaryMasters
                .Where(s => s.MonthYear != null &&
                            s.MonthYear.StartsWith(year.ToString()));

            if (!IsAdmin)
                query = query.Where(s => s.EmployeeId == empId);

            var data = await query.ToListAsync();

            var result = Enumerable.Range(1, 12).Select(m => {
                var my = $"{year}-{m:D2}";
                var md = data.Where(s => s.MonthYear == my).ToList();
                return new
                {
                    month = new DateTime(year, m, 1)
                                    .ToString("MMM"),
                    monthYear = my,
                    employees = md.Count,
                    grossSalary = md.Sum(s => s.GrossSalary ?? 0),
                    netSalary = md.Sum(s => s.NetSalary ?? 0),
                    paidCount = md.Count(s => s.Status == "Paid"),
                    generated = md.Any()
                };
            }).ToList();

            return Json(new { data = result });
        }

        // ── Employee Yearly Report ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetEmployeeYearly(
            int employeeId, int year)
        {
            if (!IsAdmin && employeeId != CurrentEmployeeId)
                return Json(new
                {
                    success = false,
                    message = "Unauthorized!"
                });

            var data = await _context.SalaryMasters
                .Include(s => s.Employee)
                .Where(s => s.EmployeeId == employeeId &&
                            s.MonthYear != null &&
                            s.MonthYear.StartsWith(year.ToString()))
                .OrderBy(s => s.MonthYear)
                .ToListAsync();

            var emp = await _context.Employees
                .FindAsync(employeeId);

            return Json(new
            {
                success = true,
                employee = new
                {
                    emp?.EmployeeCode,
                    emp?.Name,
                    emp?.Designation,
                    emp?.Department,
                    emp?.BasicSalary
                },
                data = data.Select(s => new {
                    s.Id,
                    s.MonthYear,
                    s.PresentDays,
                    s.HolidayDays,
                    s.LeaveDays,
                    s.LwpDays,
                    s.OvertimeHours,
                    s.BasicSalary,
                    s.OvertimeAmount,
                    s.GrossSalary,
                    s.Deductions,
                    s.NetSalary,
                    s.Status
                }),
                totals = new
                {
                    grossSalary = data.Sum(s => s.GrossSalary ?? 0),
                    netSalary = data.Sum(s => s.NetSalary ?? 0),
                    deductions = data.Sum(s => s.Deductions ?? 0),
                    otAmount = data.Sum(s => s.OvertimeAmount ?? 0)
                }
            });
        }

        // ── Get Employees (for dropdown) ─────────────────────
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var list = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new {
                    e.Id,
                    e.EmployeeCode,
                    e.Name
                }).ToListAsync();
            return Json(list);
        }

        // ── Salary Slip (Single) ─────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSlip(int id)
        {
            var s = await _context.SalaryMasters
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null)
                return Json(new { success = false });

            if (!IsAdmin &&
                s.EmployeeId != CurrentEmployeeId)
                return Json(new
                {
                    success = false,
                    message = "Unauthorized!"
                });

            return Json(new
            {
                success = true,
                data = new
                {
                    s.Id,
                    employeeCode = s.Employee?.EmployeeCode,
                    employeeName = s.Employee?.Name,
                    designation = s.Employee?.Designation,
                    department = s.Employee?.Department,
                    s.MonthYear,
                    s.PayableDays,
                    s.PresentDays,
                    s.HolidayDays,
                    s.LeaveDays,
                    s.LwpDays,
                    s.OvertimeHours,
                    s.BasicSalary,
                    s.OvertimeAmount,
                    s.GrossSalary,
                    s.Deductions,
                    s.NetSalary,
                    s.Status,
                    generatedDate = s.GeneratedDate?
                        .ToString("dd-MM-yyyy")
                }
            });
        }
    }
}