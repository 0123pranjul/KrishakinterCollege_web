using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace School_CRM.Controllers
{
    public class EmployeeDashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public EmployeeDashboardController(LibmanagementContext context)
        {
            _context = context;
        }

        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";

        private int CurrentEmployeeId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 0;

        public async Task<IActionResult> Index(int? employeeId)
        {
            // Fallback to cookie EmployeeId if not passed in query
            int targetId = employeeId ?? CurrentEmployeeId;

            if (targetId == 0)
            {
                // If not logged in or no employee selected
                return RedirectToAction("Login", "Account");
            }

            var employee = await _context.Employees
                .Where(e => e.Id == targetId && e.IsActive == true)
                .FirstOrDefaultAsync();

            if (employee == null)
            {
                return NotFound("Employee not found or inactive.");
            }

            ViewBag.EmployeeId = targetId;
            ViewBag.EmployeeName = employee.Name ?? "Employee";
            ViewBag.EmployeeCode = employee.EmployeeCode ?? "";
            ViewBag.Designation = employee.Designation ?? "";
            ViewBag.Department = employee.Department ?? "";
            ViewBag.IsAdmin = IsAdmin;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStats(int employeeId)
        {
            if (employeeId == 0)
            {
                return Json(new { success = false, message = "Invalid Employee ID" });
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            var thisMonthStr = DateTime.Today.ToString("yyyy-MM");

            // 1. Today's Attendance Status
            var todayAtt = await _context.AttendanceMasters
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate == today)
                .Select(a => new { a.Status, entryTime = a.EntryTime.ToString(), exitTime = a.ExitTime.ToString() })
                .FirstOrDefaultAsync();

            // 2. Upcoming Holidays (next 5)
            var upcomingHolidays = await _context.Holidays
                .Where(h => h.HolidayDate.HasValue && h.HolidayDate.Value >= today)
                .OrderBy(h => h.HolidayDate)
                .Take(5)
                .Select(h => new {
                    h.HolidayName,
                    date = h.HolidayDate.Value.ToString("dd MMM yyyy"),
                    dayName = h.HolidayDate.Value.ToDateTime(TimeOnly.MinValue).ToString("dddd")
                }).ToListAsync();

            // 3. Leave Stats & History
            var leaves = await _context.EmployeeLeaves
                .Where(l => l.EmployeeId == employeeId)
                .ToListAsync();

            var pendingLeaves = leaves.Count(l => l.Status == "Pending");
            var approvedLeaves = leaves.Count(l => l.Status == "Approved");
            var rejectedLeaves = leaves.Count(l => l.Status == "Rejected");

            var recentLeaves = leaves
                .OrderByDescending(l => l.Id)
                .Take(5)
                .Select(l => new {
                    l.Id,
                    l.LeaveType,
                    fromDate = l.FromDate.HasValue ? l.FromDate.Value.ToString("dd MMM yyyy") : "",
                    toDate = l.ToDate.HasValue ? l.ToDate.Value.ToString("dd MMM yyyy") : "",
                    l.TotalDays,
                    l.Status
                }).ToList();

            // 4. Recent Salary Slips (last 6 months)
            var salaries = await _context.SalaryMasters
                .Where(s => s.EmployeeId == employeeId)
                .OrderByDescending(s => s.MonthYear)
                .Take(6)
                .Select(s => new {
                    s.Id,
                    s.MonthYear,
                    netSalary = s.NetSalary ?? 0,
                    status = s.Status,
                    generatedDate = s.GeneratedDate.HasValue ? s.GeneratedDate.Value.ToString("dd MMM yyyy") : ""
                }).ToListAsync();

            // 5. Current Month Attendance summary
            var firstDayOfMonth = new DateOnly(today.Year, today.Month, 1);
            var monthAtt = await _context.AttendanceMasters
                .Where(a => a.EmployeeId == employeeId && a.AttendanceDate >= firstDayOfMonth && a.AttendanceDate <= today)
                .ToListAsync();

            var attendanceSummary = new
            {
                present = monthAtt.Count(a => a.Status == "Present"),
                absent = monthAtt.Count(a => a.Status == "Absent"),
                halfDay = monthAtt.Count(a => a.Status == "HalfDay"),
                leave = monthAtt.Count(a => a.Status == "Leave")
            };

            return Json(new
            {
                success = true,
                todayStatus = todayAtt?.Status ?? "Not Marked",
                todayEntry = todayAtt?.entryTime ?? "--:--",
                todayExit = todayAtt?.exitTime ?? "--:--",
                pendingLeaves,
                approvedLeaves,
                rejectedLeaves,
                upcomingHolidays,
                recentLeaves,
                salaries,
                attendanceSummary,
                todayDate = today.ToString("dd MMM yyyy")
            });
        }
    }
}
