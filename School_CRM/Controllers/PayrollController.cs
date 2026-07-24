using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Text;

namespace School_CRM.Controllers
{
    public class PayrollController : Controller
    {
        private readonly LibmanagementContext _context;

        public PayrollController(LibmanagementContext context)
        {
            _context = context;
        }

        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";

        // ── INDEX ────────────────────────────────────────────
        public IActionResult Index()
        {
            if (!IsAdmin)
                return RedirectToAction("Login", "Account");
            return View();
        }

        // ── GET SALARY LIST ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(string monthYear)
        {
            var list = await _context.SalaryMasters
                .Include(s => s.Employee)
                .Where(s => s.MonthYear == monthYear)
                .OrderBy(s => s.Employee.EmployeeCode)
                .Select(s => new {
                    s.Id,
                    employeeCode = s.Employee.EmployeeCode,
                    employeeName = s.Employee.Name,
                    designation = s.Employee.Designation,
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
                    generatedDate = s.GeneratedDate
                        .HasValue ? s.GeneratedDate.Value
                        .ToString("dd-MM-yyyy HH:mm") : ""
                }).ToListAsync();

            return Json(new { data = list });
        }

        // ── PREVIEW (Generate karne se pehle dekho) ──────────
        [HttpGet]
        public async Task<IActionResult> Preview(string monthYear)
        {
            if (!DateOnly.TryParse(monthYear + "-01", out var firstDay))
                return Json(new
                {
                    success = false,
                    message = "Invalid month!"
                });

            var result = await CalculateSalaries(monthYear, firstDay);

            // Pending advances summary for info panel
            var pendingAdvances = await _context.EmployeeAdvances
                .Include(a => a.Employee)
                .Where(a => a.Status == "Pending" &&
                            (a.DeductFromMonth == monthYear ||
                             string.IsNullOrEmpty(a.DeductFromMonth)))
                .Select(a => new {
                    a.EmployeeId,
                    employeeName = a.Employee!.Name,
                    a.Amount,
                    a.Reason
                })
                .ToListAsync();

            return Json(new {
                success = true,
                data = result,
                pendingAdvances,
                totalAdvanceAmount = pendingAdvances.Sum(a => a.Amount)
            });
        }

        // ── ONE CLICK GENERATE ───────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Generate(
            [FromBody] GenerateDto dto)
        {
            try
            {
                if (!IsAdmin)
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized!"
                    });

                if (!DateOnly.TryParse(dto.MonthYear + "-01",
                    out var firstDay))
                    return Json(new
                    {
                        success = false,
                        message = "Invalid month!"
                    });

                // Already generated check
                var alreadyExists = await _context.SalaryMasters
                    .AnyAsync(s => s.MonthYear == dto.MonthYear);

                if (alreadyExists && !dto.Regenerate)
                    return Json(new
                    {
                        success = false,
                        message = "Salary already generated! " +
                                  "Use Regenerate option."
                    });

                // Delete existing if regenerating
                if (dto.Regenerate)
                {
                    var existing = _context.SalaryMasters
                        .Where(s => s.MonthYear == dto.MonthYear);
                    _context.SalaryMasters.RemoveRange(existing);
                }

                var salaries = await CalculateSalaries(
                    dto.MonthYear, firstDay);

                foreach (var s in salaries)
                {
                    _context.SalaryMasters.Add(new SalaryMaster
                    {
                        EmployeeId = s.EmployeeId,
                        MonthYear = dto.MonthYear,
                        PayableDays = s.PayableDays,
                        PresentDays = s.PresentDays,
                        HolidayDays = s.HolidayDays,
                        LeaveDays = s.LeaveDays,
                        LwpDays = s.LwpDays,
                        OvertimeHours = s.OvertimeHours,
                        BasicSalary = s.BasicSalary,
                        OvertimeAmount = s.OvertimeAmount,
                        GrossSalary = s.GrossSalary,
                        Deductions = s.Deductions,
                        NetSalary = s.NetSalary,
                        Status = "Generated",
                        GeneratedDate = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();

                // Mark deducted advances as "Deducted"
                var allAdvanceIds = salaries
                    .SelectMany(s => s.AdvanceIds)
                    .Distinct()
                    .ToList();

                if (allAdvanceIds.Any())
                {
                    var advancesToUpdate = await _context.EmployeeAdvances
                        .Where(a => allAdvanceIds.Contains(a.Id))
                        .ToListAsync();

                    foreach (var adv in advancesToUpdate)
                    {
                        adv.Status = "Deducted";
                        adv.DeductFromMonth = dto.MonthYear;
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new
                {
                    success = true,
                    message = $"Salary generated for " +
                              $"{salaries.Count} employees!",
                    count = salaries.Count
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── UPDATE STATUS (Paid/Hold) ────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateStatus(
            [FromBody] SalaryStatusDto dto)
        {
            try
            {
                if (!IsAdmin)
                    return Json(new
                    {
                        success = false,
                        message = "Unauthorized!"
                    });

                if (dto.Id == 0)
                {
                    // Bulk update
                    var records = await _context.SalaryMasters
                        .Where(s => s.MonthYear == dto.MonthYear)
                        .ToListAsync();
                    records.ForEach(r => r.Status = dto.Status);
                }
                else
                {
                    var record = await _context.SalaryMasters
                        .FindAsync(dto.Id);
                    if (record == null)
                        return Json(new
                        {
                            success = false,
                            message = "Not found!"
                        });
                    record.Status = dto.Status;
                }

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"Status updated to {dto.Status}!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── GET SALARY SLIP ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSlip(int id)
        {
            var s = await _context.SalaryMasters
                .Include(x => x.Employee)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (s == null)
                return Json(new { success = false });

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
                    s.Status
                }
            });
        }

        // ── GET ALL SLIPS FOR MONTH (for bulk download) ──────
        [HttpGet]
        public async Task<IActionResult> GetAllSlips(string monthYear)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            var list = await _context.SalaryMasters
                .Include(x => x.Employee)
                .Where(x => x.MonthYear == monthYear)
                .OrderBy(x => x.Employee!.EmployeeCode)
                .Select(s => new
                {
                    s.Id,
                    employeeCode   = s.Employee!.EmployeeCode,
                    employeeName   = s.Employee.Name,
                    designation    = s.Employee.Designation,
                    department     = s.Employee.Department,
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
                    s.Status
                })
                .ToListAsync();

            if (!list.Any())
                return Json(new { success = false, message = "No salary data found for this month!" });

            return Json(new { success = true, data = list });
        }

        // ════════════════════════════════════════════════════
        // ── ADVANCE MANAGEMENT ───────────────────────────────
        // ════════════════════════════════════════════════════

        // ── GET ADVANCES LIST ────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAdvances(
            int? employeeId = null, string? status = null)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            var query = _context.EmployeeAdvances
                .Include(a => a.Employee)
                .AsQueryable();

            if (employeeId.HasValue && employeeId.Value > 0)
                query = query.Where(a => a.EmployeeId == employeeId);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(a => a.Status == status);

            var list = await query
                .OrderByDescending(a => a.AdvanceDate)
                .Select(a => new
                {
                    a.Id,
                    a.EmployeeId,
                    employeeCode = a.Employee!.EmployeeCode,
                    employeeName = a.Employee.Name,
                    advanceDate  = a.AdvanceDate.HasValue
                        ? a.AdvanceDate.Value.ToString("dd-MM-yyyy") : "",
                    a.Amount,
                    a.Reason,
                    a.DeductFromMonth,
                    a.Status
                })
                .ToListAsync();

            return Json(new { success = true, data = list });
        }

        // ── GET PENDING ADVANCES FOR A MONTH (preview ke liye) ──
        [HttpGet]
        public async Task<IActionResult> GetPendingAdvances(string monthYear)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            // Woh advances jo is month se deduct honge:
            // 1. DeductFromMonth == monthYear  (specifically set kiya)
            // 2. Ya DeductFromMonth is null/empty (next salary me katna hai)
            var advances = await _context.EmployeeAdvances
                .Include(a => a.Employee)
                .Where(a => a.Status == "Pending" &&
                            (a.DeductFromMonth == monthYear ||
                             string.IsNullOrEmpty(a.DeductFromMonth)))
                .Select(a => new
                {
                    a.Id,
                    a.EmployeeId,
                    employeeName = a.Employee!.Name,
                    employeeCode = a.Employee.EmployeeCode,
                    a.Amount,
                    a.Reason,
                    a.DeductFromMonth
                })
                .ToListAsync();

            return Json(new { success = true, data = advances });
        }

        // ── ADD ADVANCE ──────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddAdvance(
            [FromBody] AdvanceDto dto)
        {
            try
            {
                if (!IsAdmin)
                    return Json(new { success = false, message = "Unauthorized!" });

                if (dto.EmployeeId <= 0 || dto.Amount <= 0)
                    return Json(new { success = false, message = "Invalid data!" });

                var advance = new EmployeeAdvance
                {
                    EmployeeId      = dto.EmployeeId,
                    Amount          = dto.Amount,
                    Reason          = dto.Reason?.Trim(),
                    AdvanceDate     = dto.AdvanceDate ?? DateTime.Today,
                    DeductFromMonth = dto.DeductFromMonth,
                    Status          = "Pending",
                    CreatedDate     = DateTime.Now
                };

                _context.EmployeeAdvances.Add(advance);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Advance saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── DELETE ADVANCE ───────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteAdvance([FromBody] IdDto dto)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            var adv = await _context.EmployeeAdvances.FindAsync(dto.Id);
            if (adv == null)
                return Json(new { success = false, message = "Not found!" });

            if (adv.Status == "Deducted")
                return Json(new { success = false, message = "Already deducted — cannot delete!" });

            _context.EmployeeAdvances.Remove(adv);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Advance deleted!" });
        }

        // ── GET EMPLOYEES (for advance form dropdown) ────────
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var list = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new { e.Id, e.EmployeeCode, e.Name })
                .ToListAsync();
            return Json(new { success = true, data = list });
        }

        // ── CORE CALCULATION LOGIC ───────────────────────────
        private async Task<List<SalaryCalcResult>>
      CalculateSalaries(string monthYear, DateOnly firstDay)
        {
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            var totalDaysInMonth = lastDay.Day;

            // Sundays count
            int sundays = 0;
            for (var d = firstDay; d <= lastDay; d = d.AddDays(1))
                if (d.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;

            // Holidays table se
            var holidays = await _context.Holidays
                .Where(h => h.MonthYear == monthYear)
                .ToListAsync();

            var holidayDates = holidays
                .Where(h => h.HolidayDate.HasValue)
                .Select(h => h.HolidayDate!.Value)
                .ToHashSet();

            // Non-Sunday holidays = payable days se minus honge
            int nonSundayHolidays = holidayDates
                .Count(h => h.DayOfWeek != DayOfWeek.Sunday);

            // Payable working days
            int payableDays = totalDaysInMonth
                             - sundays
                             - nonSundayHolidays;

            // All active employees
            var employees = await _context.Employees
                .Where(e => e.IsActive == true)
                .ToListAsync();

            // Attendance
            var attendances = await _context.AttendanceMasters
                .Where(a => a.AttendanceDate >= firstDay &&
                            a.AttendanceDate <= lastDay)
                .ToListAsync();

            // Approved leaves
            var leaves = await _context.EmployeeLeaves
                .Where(l => l.Status == "Approved" &&
                            l.FromDate.HasValue &&
                            l.FromDate.Value >= firstDay &&
                            l.FromDate.Value <= lastDay)
                .ToListAsync();

            // Pending advances for this month
            var advances = await _context.EmployeeAdvances
                .Where(a => a.Status == "Pending" &&
                            (a.DeductFromMonth == monthYear ||
                             string.IsNullOrEmpty(a.DeductFromMonth)))
                .ToListAsync();

            var result = new List<SalaryCalcResult>();

            foreach (var emp in employees)
            {
                var empAtt = attendances
                    .Where(a => a.EmployeeId == emp.Id)
                    .ToList();

                // Present days
                int presentDays = empAtt
                    .Count(a => a.Status == "Present");

                // Half days
                int halfDays = empAtt
                    .Count(a => a.Status == "HalfDay");

                // Holiday days:
                // 1. Holidays table mein jo dates hain (non-Sunday)
                // 2. Ya attendance mein manually 'Holiday' mark kiya
                int holidayDaysFromTable = nonSundayHolidays;

                int holidayDaysFromAtt = empAtt
                    .Count(a => a.Status == "Holiday" &&
                                a.AttendanceDate.HasValue &&
                                !holidayDates.Contains(
                                    a.AttendanceDate.Value));

                int totalHolidayDays = holidayDaysFromTable
                                       + holidayDaysFromAtt;

                // Leave days (non-LWP)
                decimal leaveDays = leaves
                    .Where(l => l.EmployeeId == emp.Id &&
                                l.LeaveType != "LWP")
                    .Sum(l => l.TotalDays ?? 0);

                // LWP = Absent days + LWP type leaves
                int absentDays = empAtt
                    .Count(a => a.Status == "Absent");

                decimal lwpFromLeave = leaves
                    .Where(l => l.EmployeeId == emp.Id &&
                                l.LeaveType == "LWP")
                    .Sum(l => l.TotalDays ?? 0);

                decimal lwpDays = absentDays + lwpFromLeave;

                // OT hours
                decimal otHours = empAtt
                    .Sum(a => a.OvertimeHours ?? 0);

                // Salary calculation
                decimal dailyRate = emp.DailyRate ?? 0;
                decimal overtimeRate = emp.OvertimeRate ?? 0;
                decimal basicSalary = emp.BasicSalary ?? 0;

                // Earned days = Present + HalfDay*0.5 + Leave + Holiday
                decimal earnedDays = presentDays
                                   + (halfDays * 0.5m)
                                   + leaveDays
                                   + totalHolidayDays;

                decimal overtimeAmount = otHours * overtimeRate;
                decimal grossSalary = (earnedDays * dailyRate)
                                        + overtimeAmount;
                decimal lwpDeduction = lwpDays * dailyRate;

                // Advance deduction for this employee this month
                decimal advanceDeduction = advances
                    .Where(a => a.EmployeeId == emp.Id)
                    .Sum(a => a.Amount);

                var empAdvanceIds = advances
                    .Where(a => a.EmployeeId == emp.Id)
                    .Select(a => a.Id)
                    .ToList();

                decimal totalDeductions = lwpDeduction + advanceDeduction;
                decimal netSalary = grossSalary - totalDeductions;

                result.Add(new SalaryCalcResult
                {
                    EmployeeId        = emp.Id,
                    EmployeeCode      = emp.EmployeeCode ?? "",
                    EmployeeName      = emp.Name ?? "",
                    Designation       = emp.Designation ?? "",
                    PayableDays       = payableDays,
                    PresentDays       = presentDays,
                    HalfDays          = halfDays,
                    HolidayDays       = totalHolidayDays,
                    LeaveDays         = (int)leaveDays,
                    LwpDays           = (int)lwpDays,
                    OvertimeHours     = otHours,
                    BasicSalary       = basicSalary,
                    OvertimeAmount    = overtimeAmount,
                    GrossSalary       = Math.Round(grossSalary, 2),
                    LwpDeduction      = Math.Round(lwpDeduction, 2),
                    AdvanceDeduction  = Math.Round(advanceDeduction, 2),
                    Deductions        = Math.Round(totalDeductions, 2),
                    NetSalary         = Math.Round(netSalary, 2),
                    AdvanceIds        = empAdvanceIds
                });
            }

            return result;
        }
    }

    // ── DTOs ─────────────────────────────────────────────────
    public class GenerateDto
    {
        public string MonthYear { get; set; } = "";
        public bool Regenerate { get; set; } = false;
    }

    public class SalaryStatusDto
    {
        public int Id { get; set; }
        public string MonthYear { get; set; } = "";
        public string Status { get; set; } = "";
    }

    public class AdvanceDto
    {
        public int EmployeeId { get; set; }
        public decimal Amount { get; set; }
        public string? Reason { get; set; }
        public DateTime? AdvanceDate { get; set; }
        public string? DeductFromMonth { get; set; }
    }

    public class IdDto
    {
        public int Id { get; set; }
    }

    public class SalaryCalcResult
    {
        public int EmployeeId { get; set; }
        public string EmployeeCode { get; set; } = "";
        public string EmployeeName { get; set; } = "";
        public string Designation { get; set; } = "";
        public int PayableDays { get; set; }
        public int PresentDays { get; set; }
        public int HalfDays { get; set; }
        public int HolidayDays { get; set; }
        public int LeaveDays { get; set; }
        public int LwpDays { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal BasicSalary { get; set; }
        public decimal OvertimeAmount { get; set; }
        public decimal GrossSalary { get; set; }
        public decimal LwpDeduction { get; set; }
        public decimal AdvanceDeduction { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public List<int> AdvanceIds { get; set; } = new();
    }
}