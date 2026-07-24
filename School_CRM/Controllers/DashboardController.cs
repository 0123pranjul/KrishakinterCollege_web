using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class DashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public DashboardController(LibmanagementContext context)
        {
            _context = context;
        }

        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";
        private int CurrentEmployeeId =>
            int.TryParse(
                HttpContext.Request.Cookies["EmployeeId"],
                out var id) ? id : 0;

        public IActionResult Index()
        {
            ViewBag.IsAdmin = IsAdmin;
            ViewBag.EmployeeName =
                HttpContext.Request.Cookies["EmployeeName"] ?? "User";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var monthYear = DateTime.Today.ToString("yyyy-MM");
            var empId = CurrentEmployeeId;
            var isAdmin = IsAdmin;

            // ── Employee Stats ───────────────────────────────
            var totalEmp = await _context.Employees
                .CountAsync(e => e.IsActive == true);
            var activeEmp = totalEmp;

            // ── Today Attendance ─────────────────────────────
            var todayAtt = await _context.AttendanceMasters
                .Where(a => a.AttendanceDate == today)
                .ToListAsync();

            int presentToday = todayAtt.Count(
                a => a.Status == "Present");
            int absentToday = totalEmp - presentToday;
            int halfDayToday = todayAtt.Count(
                a => a.Status == "HalfDay");
            int onLeaveToday = todayAtt.Count(
                a => a.Status == "Leave");

            // ── This Month Attendance ────────────────────────
            var firstDay = new DateOnly(
                today.Year, today.Month, 1);
            var monthAtt = await _context.AttendanceMasters
                .Where(a => a.AttendanceDate >= firstDay &&
                            a.AttendanceDate <= today)
                .ToListAsync();

            // ── Leave Stats ──────────────────────────────────
            var leaveQuery = _context.EmployeeLeaves
                .AsQueryable();
            if (!isAdmin)
                leaveQuery = leaveQuery.Where(
                    l => l.EmployeeId == empId);

            var pendingLeaves = await leaveQuery
                .CountAsync(l => l.Status == "Pending");
            var approvedLeavesMonth = await leaveQuery
                .CountAsync(l => l.Status == "Approved" &&
                    l.FromDate.HasValue &&
                    l.FromDate.Value.Month == today.Month &&
                    l.FromDate.Value.Year == today.Year);

            // ── Salary Stats ─────────────────────────────────
            var salaryQuery = _context.SalaryMasters
                .Where(s => s.MonthYear == monthYear);
            if (!isAdmin)
                salaryQuery = salaryQuery.Where(
                    s => s.EmployeeId == empId);

            var salaries = await salaryQuery.ToListAsync();
            decimal totalNet = salaries.Sum(s => s.NetSalary ?? 0);
            decimal totalPaid = salaries
                .Where(s => s.Status == "Paid")
                .Sum(s => s.NetSalary ?? 0);
            int paidCount = salaries.Count(
                s => s.Status == "Paid");
            int pendingCount = salaries.Count(
                s => s.Status != "Paid");

            // ── Holiday Stats ────────────────────────────────
            var upcomingHolidays = await _context.Holidays
                .Where(h => h.HolidayDate.HasValue &&
                            h.HolidayDate.Value >= today)
                .OrderBy(h => h.HolidayDate)
                .Take(3)
                .Select(h => new {
                    h.HolidayName,
                    date = h.HolidayDate.ToString(),
                    dayName = h.HolidayDate.HasValue
                        ? h.HolidayDate.Value
                            .ToDateTime(TimeOnly.MinValue)
                            .ToString("ddd, dd MMM") : ""
                }).ToListAsync();

            // ── Recent Activities ────────────────────────────
            var recentAtt = await _context.AttendanceMasters
                .Include(a => a.Employee)
                .OrderByDescending(a => a.AttendanceDate)
                .Take(5)
                .Select(a => new {
                    type = "attendance",
                    description = $"{a.Employee.Name} - " +
                                  $"{a.Status}",
                    date = a.AttendanceDate.ToString(),
                    status = a.Status
                }).ToListAsync();

            var recentLeaves = await _context.EmployeeLeaves
                .Include(l => l.Employee)
                .OrderByDescending(l => l.Id)
                .Take(5)
                .Select(l => new {
                    type = "leave",
                    description = $"{l.Employee.Name} - " +
                                  $"{l.LeaveType} Leave",
                    date = l.FromDate.ToString(),
                    status = l.Status
                }).ToListAsync();

            // ── Monthly Attendance Pie Data ──────────────────
            var monthlyPie = new
            {
                present = monthAtt.Count(a => a.Status == "Present"),
                absent = monthAtt.Count(a => a.Status == "Absent"),
                halfDay = monthAtt.Count(a => a.Status == "HalfDay"),
                leave = monthAtt.Count(a => a.Status == "Leave"),
                holiday = monthAtt.Count(a => a.Status == "Holiday")
            };

            // ── Department wise ──────────────────────────────
            var deptStats = await _context.Employees
                .Where(e => e.IsActive == true)
                .GroupBy(e => e.Department)
                .Select(g => new {
                    department = g.Key,
                    count = g.Count()
                }).ToListAsync();

            return Json(new
            {
                totalEmp,
                presentToday,
                absentToday,
                halfDayToday,
                onLeaveToday,
                pendingLeaves,
                approvedLeavesMonth,
                totalNet,
                totalPaid,
                paidCount,
                pendingCount,
                upcomingHolidays,
                recentAtt,
                recentLeaves,
                monthlyPie,
                deptStats,
                monthYear,
                todayDate = today.ToString("dd MMM yyyy")
            });
        }

        // ── Monthly Attendance Trend (last 7 days) ────────────
        [HttpGet]
        public async Task<IActionResult> GetAttendanceTrend()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = today.AddDays(-6);

            var att = await _context.AttendanceMasters
                .Where(a => a.AttendanceDate >= from &&
                            a.AttendanceDate <= today)
                .ToListAsync();

            var result = Enumerable.Range(0, 7).Select(i => {
                var d = from.AddDays(i);
                var dayA = att.Where(
                    a => a.AttendanceDate == d).ToList();
                return new
                {
                    date = d.ToString("dd MMM"),
                    present = dayA.Count(a => a.Status == "Present"),
                    absent = dayA.Count(a => a.Status == "Absent"),
                    halfDay = dayA.Count(a => a.Status == "HalfDay")
                };
            }).ToList();

            return Json(result);
        }
    }
}