using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ActivityCalendarController : Controller
    {
        private readonly LibmanagementContext _context;

        public ActivityCalendarController(
            LibmanagementContext context)
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
                HttpContext.Request.Cookies["EmployeeName"] ?? "";
            return View();
        }

        // ── Employee List (Admin only) ────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var list = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new {
                    e.Id,
                    e.EmployeeCode,
                    e.Name,
                    e.Designation,
                    e.Department
                }).ToListAsync();
            return Json(list);
        }

        // ── Calendar Data ─────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetCalendarData(
            int? employeeId, string monthYear)
        {
            var isAdmin = IsAdmin;
            var empId = employeeId ?? CurrentEmployeeId;

            // Employee sirf apna dekhe
            if (!isAdmin) empId = CurrentEmployeeId;

            if (!DateOnly.TryParse(monthYear + "-01",
                out var firstDay))
                return Json(new
                {
                    success = false,
                    message = "Invalid month!"
                });

            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            // Employee info
            var emp = await _context.Employees
                .FindAsync(empId);
            if (emp == null)
                return Json(new
                {
                    success = false,
                    message = "Employee not found!"
                });

            // Attendance
            var attendance = await _context.AttendanceMasters
                .Where(a => a.EmployeeId == empId &&
                            a.AttendanceDate >= firstDay &&
                            a.AttendanceDate <= lastDay)
                .ToListAsync();

            // Approved Leaves
            var leaves = await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == empId &&
                            l.Status == "Approved" &&
                            l.FromDate.HasValue &&
                            l.ToDate.HasValue &&
                            l.FromDate.Value <= lastDay &&
                            l.ToDate.Value >= firstDay)
                .ToListAsync();

            // Holidays
            var holidays = await _context.Holidays
                .Where(h => h.MonthYear == monthYear)
                .ToListAsync();

            // Salary
            var salary = await _context.SalaryMasters
                .FirstOrDefaultAsync(s =>
                    s.EmployeeId == empId &&
                    s.MonthYear == monthYear);

            // Build calendar days
            var calendarDays = new List<object>();

            for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
            {
                var att = attendance.FirstOrDefault(
                    a => a.AttendanceDate == d);

                // Check leave
                var leave = leaves.FirstOrDefault(l =>
                    l.FromDate.HasValue && l.ToDate.HasValue &&
                    d >= l.FromDate.Value &&
                    d <= l.ToDate.Value);

                // Check holiday
                var holiday = holidays.FirstOrDefault(
                    h => h.HolidayDate == d);

                // Determine status
                string status;
                string leaveType = "";
                decimal hoursWorked = 0;
                decimal overtimeHrs = 0;

                if (d.DayOfWeek == DayOfWeek.Sunday)
                {
                    status = "Sunday";
                }
                else if (holiday != null)
                {
                    status = "Holiday";
                }
                else if (att != null)
                {
                    status = att.Status ?? "Absent";
                    hoursWorked = att.HoursWorked ?? 0;
                    overtimeHrs = att.OvertimeHours ?? 0;
                    if (leave != null)
                        leaveType = leave.LeaveType ?? "";
                }
                else if (leave != null)
                {
                    status = "Leave";
                    leaveType = leave.LeaveType ?? "";
                }
                else if (d > DateOnly.FromDateTime(DateTime.Today))
                {
                    status = "Future";
                }
                else
                {
                    status = "Absent";
                }

                calendarDays.Add(new
                {
                    date = d.ToString("yyyy-MM-dd"),
                    day = d.Day,
                    dayName = d.ToString("ddd"),
                    status,
                    leaveType,
                    hoursWorked,
                    overtimeHrs,
                    holidayName = holiday?.HolidayName ?? "",
                    isToday = d == DateOnly.FromDateTime(
                                      DateTime.Today),
                    isSunday = d.DayOfWeek ==
                                  DayOfWeek.Sunday
                });
            }

            // Summary
            var summary = new
            {
                presentDays = attendance.Count(
                    a => a.Status == "Present"),
                halfDays = attendance.Count(
                    a => a.Status == "HalfDay"),
                absentDays = attendance.Count(
                    a => a.Status == "Absent"),
                leaveDays = leaves.Sum(l => l.TotalDays ?? 0),
                holidayDays = holidays.Count(h =>
                    h.HolidayDate.HasValue &&
                    h.HolidayDate.Value.DayOfWeek !=
                    DayOfWeek.Sunday),
                sundayCount = calendarDays.Count(
                    c => ((dynamic)c).status == "Sunday"),
                totalOTHours = attendance.Sum(
                    a => a.OvertimeHours ?? 0),
                totalHours = attendance.Sum(
                    a => a.HoursWorked ?? 0),
                casualLeave = leaves.Where(l =>
                    l.LeaveType == "CasualLeave")
                    .Sum(l => l.TotalDays ?? 0),
                sickLeave = leaves.Where(l =>
                    l.LeaveType == "SickLeave")
                    .Sum(l => l.TotalDays ?? 0),
                earnedLeave = leaves.Where(l =>
                    l.LeaveType == "EarnedLeave")
                    .Sum(l => l.TotalDays ?? 0),
                salary = salary == null ? null : new
                {
                    salary.GrossSalary,
                    salary.NetSalary,
                    salary.Deductions,
                    salary.OvertimeAmount,
                    salary.Status
                }
            };

            return Json(new
            {
                success = true,
                employee = new
                {
                    emp.Id,
                    emp.EmployeeCode,
                    emp.Name,
                    emp.Designation,
                    emp.Department,
                    emp.BasicSalary
                },
                calendarDays,
                summary,
                firstDayOfWeek = (int)firstDay.DayOfWeek
            });
        }
    }
}