using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class LeaveController : Controller
    {
        private readonly LibmanagementContext _context;

        public LeaveController(LibmanagementContext context)
        {
            _context = context;
        }

        // Helper — session se values
        private int CurrentEmployeeId =>
        int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 0;

        private bool IsAdmin =>
     HttpContext.Request.Cookies["IsAdmin"] == "true";

        // ─── INDEX ───────────────────────────────────────────
        public IActionResult Index()
        {
            // Admin ke liye EmployeeId 0 ho sakta hai - allow karo
            var empId = CurrentEmployeeId;
            var isAdmin = IsAdmin;

            if (empId == 0 && !isAdmin)
                return RedirectToAction("Login", "Account");

            ViewBag.IsAdmin = isAdmin;
            return View();
        }

        // ─── GET ALL LEAVES ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var empId = CurrentEmployeeId;
            var isAdmin = IsAdmin;

            // Login check
            if (empId == 0 && !isAdmin)
                return Json(new { data = new List<object>() });

            var query = _context.EmployeeLeaves
                .Include(l => l.Employee)
                .AsQueryable();

            // Employee sirf apni leaves dekhe
            if (!isAdmin)
                query = query.Where(l => l.EmployeeId == empId);

            var leaves = await query
                .OrderByDescending(l => l.FromDate)
                .Select(l => new {
                    l.Id,
                    employeeCode = l.Employee.EmployeeCode,
                    employeeName = l.Employee.Name,
                    l.LeaveType,
                    fromDate = l.FromDate.ToString(),
                    toDate = l.ToDate.ToString(),
                    l.TotalDays,
                    l.Status
                }).ToListAsync();

            return Json(new { data = leaves });
        }
        // ─── GET BY ID ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return Json(new
                {
                    id = 0,
                    employeeId = IsAdmin ? 0 : CurrentEmployeeId,
                    leaveType = "CasualLeave",
                    fromDate = "",
                    toDate = "",
                    totalDays = 0,
                    status = "Pending"
                });

            var l = await _context.EmployeeLeaves.FindAsync(id);
            if (l == null) return NotFound();

            // Employee apni hi leave edit kar sakta hai
            if (!IsAdmin && l.EmployeeId != CurrentEmployeeId)
                return Json(new { error = "Unauthorized" });

            return Json(new
            {
                l.Id,
                l.EmployeeId,
                l.LeaveType,
                fromDate = l.FromDate.ToString(),
                toDate = l.ToDate.ToString(),
                l.TotalDays,
                l.Status
            });
        }

        // ─── SAVE ────────────────────────────────────────────
        // ─── SAVE ────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Save([FromBody] LeaveDto dto)
        {
            try
            {
                var empId = CurrentEmployeeId;
                var isAdmin = IsAdmin;

                // Login check
                if (empId == 0 && !isAdmin)
                    return Json(new { success = false, message = "Not logged in!" });

                // Employee sirf apne liye apply kar sakta hai
                if (!isAdmin)
                    dto.EmployeeId = empId;
                else if (dto.EmployeeId <= 0)
                    return Json(new { success = false, message = "Select employee!" });

                if (!DateOnly.TryParse(dto.FromDate, out var from) ||
                    !DateOnly.TryParse(dto.ToDate, out var to))
                    return Json(new { success = false, message = "Invalid dates!" });

                if (to < from)
                    return Json(new { success = false, message = "To date must be after From date!" });

                // ── Balance check (Employee ke liye) ──────────────
                if (!isAdmin)
                {
                    var currentYear = DateTime.Today.Year;
                    var usedDays = await _context.EmployeeLeaves
                        .Where(l => l.EmployeeId == empId &&
                                    l.LeaveType == dto.LeaveType &&
                                    l.Status == "Approved" &&
                                    l.FromDate.HasValue &&
                                    l.FromDate.Value.Year == currentYear &&
                                    (dto.Id == 0 || l.Id != dto.Id))
                        .SumAsync(l => l.TotalDays ?? 0);

                    decimal maxAllowed = dto.LeaveType switch
                    {
                        "CasualLeave" => 12,
                        "SickLeave" => 12,
                        "EarnedLeave" => 15,
                        _ => 999
                    };

                    decimal requestedDays = to.DayNumber - from.DayNumber + 1;

                    if (usedDays + requestedDays > maxAllowed)
                        return Json(new
                        {
                            success = false,
                            message = $"Insufficient balance! Used: {usedDays}, " +
                                      $"Requesting: {requestedDays}, Max: {maxAllowed}"
                        });
                }

                decimal totalDays = to.DayNumber - from.DayNumber + 1;

                if (dto.Id == 0)
                {
                    // Duplicate check
                    var duplicate = await _context.EmployeeLeaves.AnyAsync(l =>
                        l.EmployeeId == dto.EmployeeId &&
                        l.Status != "Rejected" &&
                        ((l.FromDate <= from && l.ToDate >= from) ||
                         (l.FromDate <= to && l.ToDate >= to)));

                    if (duplicate)
                        return Json(new
                        {
                            success = false,
                            message = "Leave already applied for this period!"
                        });

                    _context.EmployeeLeaves.Add(new EmployeeLeaf
                    {
                        EmployeeId = dto.EmployeeId,
                        LeaveType = dto.LeaveType,
                        FromDate = from,
                        ToDate = to,
                        TotalDays = totalDays,
                        Status = "Pending"
                    });
                }
                else
                {
                    var existing = await _context.EmployeeLeaves.FindAsync(dto.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Not found!" });

                    if (!isAdmin && existing.Status != "Pending")
                        return Json(new
                        {
                            success = false,
                            message = "Cannot edit approved/rejected leave!"
                        });

                    existing.EmployeeId = dto.EmployeeId;
                    existing.LeaveType = dto.LeaveType;
                    existing.FromDate = from;
                    existing.ToDate = to;
                    existing.TotalDays = totalDays;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Leave saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── APPROVE / REJECT (Admin only) ───────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateStatus([FromBody] LeaveStatusDto dto)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized!" });

            try
            {
                var leave = await _context.EmployeeLeaves.FindAsync(dto.Id);
                if (leave == null)
                    return Json(new { success = false, message = "Not found!" });

                leave.Status = dto.Status;
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Leave {dto.Status}!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── DELETE ──────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            try
            {
                var leave = await _context.EmployeeLeaves.FindAsync(id);
                if (leave == null)
                    return Json(new { success = false, message = "Not found!" });

                // Employee sirf apni Pending leave delete kar sakta hai
                if (!IsAdmin)
                {
                    if (leave.EmployeeId != CurrentEmployeeId)
                        return Json(new { success = false, message = "Unauthorized!" });
                    if (leave.Status != "Pending")
                        return Json(new
                        {
                            success = false,
                            message = "Cannot delete approved/rejected leave!"
                        });
                }

                _context.EmployeeLeaves.Remove(leave);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ─── LEAVE BALANCE ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetLeaveBalance()
        {
            var empId = CurrentEmployeeId;
            var isAdmin = IsAdmin;

            // Admin ya logged in employee hona chahiye
            if (empId == 0 && !isAdmin)
                return Json(new { data = new List<object>() });

            var currentYear = DateTime.Today.Year;

            var empQuery = _context.Employees.Where(e => e.IsActive == true);

            // Employee sirf apna balance dekhe
            if (!isAdmin)
                empQuery = empQuery.Where(e => e.Id == empId);

            var employees = await empQuery
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new { e.Id, e.EmployeeCode, e.Name })
                .ToListAsync();

            var empIds = employees.Select(e => e.Id).ToList();

            var leaves = await _context.EmployeeLeaves
                .Where(l => l.EmployeeId.HasValue &&
                            empIds.Contains(l.EmployeeId.Value) &&
                            l.FromDate.HasValue &&
                            l.FromDate.Value.Year == currentYear &&
                            l.Status == "Approved")
                .ToListAsync();

            var result = employees.Select(emp => {
                var el = leaves.Where(l => l.EmployeeId == emp.Id).ToList();
                decimal casual = el.Where(l => l.LeaveType == "CasualLeave").Sum(l => l.TotalDays ?? 0);
                decimal sick = el.Where(l => l.LeaveType == "SickLeave").Sum(l => l.TotalDays ?? 0);
                decimal earned = el.Where(l => l.LeaveType == "EarnedLeave").Sum(l => l.TotalDays ?? 0);

                return new
                {
                    employeeCode = emp.EmployeeCode,
                    employeeName = emp.Name,
                    casualUsed = casual,
                    casualBalance = Math.Max(0, 12 - casual),
                    sickUsed = sick,
                    sickBalance = Math.Max(0, 12 - sick),
                    earnedUsed = earned,
                    earnedBalance = Math.Max(0, 15 - earned),
                    totalUsed = casual + sick + earned
                };
            }).ToList();

            return Json(new { data = result });
        }
        // ─── EMPLOYEES LIST (Admin only) ─────────────────────
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var list = await _context.Employees
                .Where(e => e.IsActive == true)
                .OrderBy(e => e.EmployeeCode)
                .Select(e => new { e.Id, e.EmployeeCode, e.Name })
                .ToListAsync();
            return Json(list);
        }
    }

    public class LeaveDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string? LeaveType { get; set; }
        public string? FromDate { get; set; }
        public string? ToDate { get; set; }
    }

    public class LeaveStatusDto
    {
        public int Id { get; set; }
        public string? Status { get; set; }
    }
}