using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using School_CRM.Models.ViewModels;
using System.Security.Claims;

namespace School_CRM.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,Teacher,Principal")]
    public class TeacherScheduleController : Controller
    {
        private readonly LibmanagementContext _context;

        public TeacherScheduleController(LibmanagementContext context)
        {
            _context = context;
        }

        // ─────────────────────────────────────────────
        //  GET: /TeacherSchedule/Index
        // ─────────────────────────────────────────────
        public async Task<IActionResult> Index(int? teacherId, int? sessionId)
        {
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            bool isSuperAdmin = role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)
                             || role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                             || role.Equals("Principal", StringComparison.OrdinalIgnoreCase);

            // ── Resolve which teacher to show ──────────────────────────
            int resolvedTeacherId = 0;

            if (isSuperAdmin)
            {
                resolvedTeacherId = teacherId ?? 0;
                ViewBag.Teachers = await _context.TblTeachers
                    .Where(t => t.IsActive == true)
                    .OrderBy(t => t.TeacherName)
                    .ToListAsync();
            }
            else
            {
                // Teacher: get TeacherId from cookie set during login
                var teacherIdCookie = Request.Cookies["EntityId"];
                if (int.TryParse(teacherIdCookie, out int tid))
                    resolvedTeacherId = tid;
            }

            ViewBag.IsSuperAdmin      = isSuperAdmin;
            ViewBag.SelectedTeacherId = resolvedTeacherId;

            // ── Sessions ───────────────────────────────────────────────
            var sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            int resolvedSessionId = sessionId
                ?? sessions.FirstOrDefault()?.SessionId
                ?? 0;

            ViewBag.Sessions          = sessions;
            ViewBag.SelectedSessionId = resolvedSessionId;

            // ── Build schedule if teacher + session are known ──────────
            ScheduleViewModel? schedule = null;
            if (resolvedTeacherId > 0 && resolvedSessionId > 0)
                schedule = await BuildSchedule(resolvedTeacherId, resolvedSessionId);

            return View(schedule);
        }

        // ─────────────────────────────────────────────
        //  AJAX: GET /TeacherSchedule/GetSchedule
        // ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSchedule(int teacherId, int sessionId)
        {
            if (teacherId <= 0 || sessionId <= 0)
                return Json(new { success = false, message = "Invalid parameters" });

            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? "";
            bool isSuperAdmin = role.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)
                             || role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                             || role.Equals("Principal", StringComparison.OrdinalIgnoreCase);

            if (!isSuperAdmin)
            {
                var cookieTeacherId = Request.Cookies["EntityId"];
                if (!int.TryParse(cookieTeacherId, out int myId) || myId != teacherId)
                    return Forbid();
            }

            var schedule = await BuildSchedule(teacherId, sessionId);
            return Json(new { success = true, data = schedule });
        }

        // ─────────────────────────────────────────────
        //  Core: Build weekly schedule grid
        // ─────────────────────────────────────────────
        private async Task<ScheduleViewModel> BuildSchedule(int teacherId, int sessionId)
        {
            var teacher = await _context.TblTeachers
                .Where(t => t.TeacherId == teacherId)
                .Select(t => new { t.TeacherId, t.TeacherName, t.Designation })
                .FirstOrDefaultAsync();

            var periods = await _context.TblPeriods
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.SequenceNo)
                .ToListAsync();

            var entries = await _context.TblTimeTables
                .Where(tt => tt.TeacherId == teacherId
                          && tt.SessionId == sessionId
                          && tt.IsActive == true)
                .Include(tt => tt.Class)
                .Include(tt => tt.Section)
                .Include(tt => tt.Subject)
                .Include(tt => tt.Period)
                .ToListAsync();

            var dayStats = Enumerable.Range(1, 6).Select(day => new DayStatViewModel
            {
                DayNumber  = day,
                DayName    = DayName(day),
                DayShort   = DayShort(day),
                TotalSlots = entries.Count(e => e.DayOfWeek == day)
            }).ToList();

            var rows = periods.Select(p => new PeriodRowViewModel
            {
                PeriodId   = p.PeriodId,
                PeriodName = p.PeriodName,
                StartTime  = p.StartTime.ToString("hh\\:mm tt"),
                EndTime    = p.EndTime.ToString("hh\\:mm tt"),
                IsBrake    = p.IsBrake ?? false,
                Cells      = Enumerable.Range(1, 6).Select(day =>
                {
                    var entry = entries.FirstOrDefault(e => e.PeriodId == p.PeriodId && e.DayOfWeek == day);
                    return new ScheduleCellViewModel
                    {
                        DayNumber   = day,
                        HasEntry    = entry != null,
                        ClassName   = entry?.Class?.ClassName ?? "",
                        SectionName = entry?.Section?.SectionName ?? "",
                        SubjectName = entry?.Subject?.SubjectName ?? "",
                        TimeTableId = entry?.TimeTableId ?? 0
                    };
                }).ToList()
            }).ToList();

            return new ScheduleViewModel
            {
                TeacherId           = teacherId,
                TeacherName         = teacher?.TeacherName ?? "Unknown",
                TeacherDesignation  = teacher?.Designation ?? "",
                SessionId           = sessionId,
                TotalPeriodsWeek    = entries.Count,
                DayStats            = dayStats,
                PeriodRows          = rows
            };
        }

        private static string DayName(int day) => day switch
        {
            1 => "Monday", 2 => "Tuesday", 3 => "Wednesday",
            4 => "Thursday", 5 => "Friday", 6 => "Saturday",
            _ => ""
        };

        private static string DayShort(int day) => day switch
        {
            1 => "Mon", 2 => "Tue", 3 => "Wed",
            4 => "Thu", 5 => "Fri", 6 => "Sat",
            _ => ""
        };
    }
}
