using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly LibmanagementContext _context;

        public AttendanceController(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateOnly? selectedDate)
        {
            ViewBag.SelectedDate = selectedDate ?? DateOnly.FromDateTime(DateTime.Today);
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTodayAttendance(string date)
        {
            if (!DateOnly.TryParse(date, out var parsedDate))
                parsedDate = DateOnly.FromDateTime(DateTime.Today);

            var attendances = await _context.AttendanceMasters
                .Where(a => a.AttendanceDate == parsedDate)
                .Select(a => new {
                    a.Id,
                    a.EmployeeId,
                    EmployeeCode = a.Employee.EmployeeCode,
                    EmployeeName = a.Employee.Name,
                    a.Status,
                    a.HoursWorked,
                    a.OvertimeHours
                }).ToListAsync();

            var allEmployees = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new { e.Id, e.EmployeeCode, e.Name })
                .ToListAsync();

            var result = allEmployees.Select(emp => {
                var att = attendances.FirstOrDefault(a => a.EmployeeId == emp.Id);
                return new
                {
                    id = att?.Id ?? 0,
                    employeeId = emp.Id,
                    employeeCode = emp.EmployeeCode,
                    employeeName = emp.Name,
                    status = att?.Status ?? "Absent",
                    hoursWorked = att?.HoursWorked ?? 0,
                    overtimeHours = att?.OvertimeHours ?? 0
                };
            }).ToList();

            return Json(new { data = result });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveAttendance([FromBody] List<AttendanceDto> attendances)
        {
            try
            {
                if (attendances == null || attendances.Count == 0)
                    return Json(new { success = false, message = "No data received!" });

                foreach (var dto in attendances)
                {
                    if (dto.EmployeeId <= 0) continue;
                    if (!DateOnly.TryParse(dto.AttendanceDate, out var attDate)) continue;

                    var existing = await _context.AttendanceMasters
                        .FirstOrDefaultAsync(a =>
                            a.EmployeeId == dto.EmployeeId &&
                            a.AttendanceDate == attDate);

                    if (existing != null)
                    {
                        existing.Status = dto.Status;
                        existing.HoursWorked = dto.HoursWorked;
                        existing.OvertimeHours = dto.OvertimeHours;
                    }
                    else if (dto.Status != "Absent")
                    {
                        _context.AttendanceMasters.Add(new AttendanceMaster
                        {
                            EmployeeId = dto.EmployeeId,
                            AttendanceDate = attDate,
                            Status = dto.Status,
                            HoursWorked = dto.HoursWorked,
                            OvertimeHours = dto.OvertimeHours
                        });
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Attendance saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ===== REPORT =====
        [HttpGet]
        public IActionResult Report()
        {
            ViewBag.FromDate = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            ViewBag.ToDate = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAttendanceReport(string fromDate, string toDate)
        {
            if (!DateOnly.TryParse(fromDate, out var from)) from = DateOnly.FromDateTime(DateTime.Today.AddDays(-30));
            if (!DateOnly.TryParse(toDate, out var to)) to = DateOnly.FromDateTime(DateTime.Today);

            var report = await _context.AttendanceMasters
                .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to)
                .Include(a => a.Employee)
                .GroupBy(a => new { a.EmployeeId, a.Employee.EmployeeCode, a.Employee.Name })
                .Select(g => new {
                    employeeCode = g.Key.EmployeeCode,
                    employeeName = g.Key.Name,
                    totalDays = g.Count(),
                    presentDays = g.Count(a => a.Status == "Present"),
                    halfDays = g.Count(a => a.Status == "HalfDay"),
                    leaveDays = g.Count(a => a.Status == "Leave"),
                    holidayDays = g.Count(a => a.Status == "Holiday"),
                    absentDays = g.Count(a => a.Status == "Absent"),
                    totalHours = g.Sum(a => a.HoursWorked ?? 0),
                    totalOT = g.Sum(a => a.OvertimeHours ?? 0)
                })
                .OrderBy(r => r.employeeCode)
                .ToListAsync();

            return Json(new { data = report });
        }

    
        public class AttendanceDto
        {
            public int Id { get; set; }
            public int EmployeeId { get; set; }
            public string? AttendanceDate { get; set; }
            public string? Status { get; set; }
            public decimal HoursWorked { get; set; }
            public decimal OvertimeHours { get; set; }
        }
    }

}