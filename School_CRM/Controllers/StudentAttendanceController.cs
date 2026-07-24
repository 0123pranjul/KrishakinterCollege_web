using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudentAttendanceController : Controller
    {
        private readonly LibmanagementContext _context;
        public StudentAttendanceController(LibmanagementContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForAttendance(int sessionId, int classId, int sectionId, string date)
        {
            if (!DateOnly.TryParse(date, out var attendanceDate))
                return Json(new { success = false, message = "Invalid date!" });

            var students = await _context.TblStudentSessions
                .Where(ss => ss.SessionId == sessionId && ss.ClassId == classId && ss.SectionId == sectionId && ss.IsActive == true && ss.StudentId != null)
                .Include(ss => ss.Student)
                .ToListAsync();

            var existing = await _context.TblStudentAttendances
                .Where(a => a.SessionId == sessionId && a.ClassId == classId && a.SectionId == sectionId && a.AttendanceDate == attendanceDate && a.IsActive == true)
                .ToListAsync();

            var data = students.Select(ss => {
                var att = existing.FirstOrDefault(a => a.StudentId == ss.StudentId!.Value);
                return new
                {
                    StudentId = ss.StudentId!.Value,
                    StudentName = ss.Student?.StudentName ?? "-",
                    AttendanceId = att?.AttendanceId ?? 0,
                    Status = att?.Status ?? "Present"
                };
            }).ToList();

            return Json(new { data, alreadySaved = existing.Any() });
        }

        [HttpPost]
        public async Task<IActionResult> SaveAttendance([FromBody] AttendanceSaveRequest request)
        {
            try
            {
                if (!DateOnly.TryParse(request.Date, out var attendanceDate))
                    return Json(new { success = false, message = "Invalid date!" });

                foreach (var entry in request.Entries)
                {
                    // First check existing attendance
                    var existingAttendance = await _context.TblStudentAttendances
                        .FirstOrDefaultAsync(x =>
                            x.StudentId == entry.StudentId &&
                            x.SessionId == request.SessionId &&
                            x.AttendanceDate == attendanceDate);

                    if (existingAttendance == null)
                    {
                        // INSERT
                        _context.TblStudentAttendances.Add(new TblStudentAttendance
                        {
                            StudentId = entry.StudentId,
                            SessionId = request.SessionId,
                            ClassId = request.ClassId,
                            SectionId = request.SectionId,
                            AttendanceDate = attendanceDate,
                            Status = entry.Status,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        // UPDATE
                        existingAttendance.Status = entry.Status;
                        existingAttendance.ClassId = request.ClassId;
                        existingAttendance.SectionId = request.SectionId;
                        existingAttendance.UpdatedDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Attendance saved successfully!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetReport(int sessionId, int classId, int sectionId, string fromDate, string toDate)
        {
            if (!DateOnly.TryParse(fromDate, out var from) || !DateOnly.TryParse(toDate, out var to))
                return Json(new { success = false, message = "Invalid date range!" });

            var data = await _context.TblStudentAttendances
                .Where(a => a.SessionId == sessionId && a.ClassId == classId && a.SectionId == sectionId
                    && a.AttendanceDate >= from && a.AttendanceDate <= to && a.IsActive == true)
                .Include(a => a.Student)
                .GroupBy(a => new { a.StudentId, a.Student.StudentName })
                .Select(g => new
                {
                    g.Key.StudentId,
                    g.Key.StudentName,
                    TotalDays  = g.Count(),
                    Present    = g.Count(a => a.Status == "Present"),
                    Absent     = g.Count(a => a.Status == "Absent"),
                    Late       = g.Count(a => a.Status == "Late"),
                    Percentage = g.Count() > 0 ? Math.Round((double)g.Count(a => a.Status == "Present") / g.Count() * 100, 2) : 0
                }).ToListAsync();

            return Json(new { data });
        }

        // ── Monthly Grid: rows = students, columns = dates ────────────────────
        [HttpGet]
        public async Task<IActionResult> GetMonthlyGrid(int sessionId, int classId, int sectionId, int year, int month)
        {
            var from = new DateOnly(year, month, 1);
            var to   = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            var students = await _context.TblStudentSessions
                .Where(ss => ss.SessionId == sessionId && ss.ClassId == classId
                          && ss.SectionId == sectionId && ss.IsActive == true
                          && ss.StudentId != null)
                .Include(ss => ss.Student)
                .OrderBy(ss => ss.Student!.StudentName)
                .Select(ss => new { ss.StudentId, ss.Student!.StudentName, ss.Student!.RollNo })
                .ToListAsync();

            var records = await _context.TblStudentAttendances
                .Where(a => a.SessionId == sessionId && a.ClassId == classId
                         && a.SectionId == sectionId
                         && a.AttendanceDate >= from && a.AttendanceDate <= to
                         && a.IsActive == true)
                .Select(a => new { a.StudentId, a.AttendanceDate, a.Status })
                .ToListAsync();

            var markedDates = records.Select(r => r.AttendanceDate).Distinct().OrderBy(d => d).ToList();

            var rows = students.Select(s =>
            {
                var attMap = records
                    .Where(r => r.StudentId == s.StudentId)
                    .ToDictionary(r => r.AttendanceDate, r => r.Status);

                int present = attMap.Values.Count(v => v == "Present");
                int absent  = attMap.Values.Count(v => v == "Absent");
                int late    = attMap.Values.Count(v => v == "Late");
                int total   = markedDates.Count;
                double pct  = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;

                return new
                {
                    s.StudentId,
                    s.StudentName,
                    RollNo     = s.RollNo ?? "-",
                    Present    = present,
                    Absent     = absent,
                    Late       = late,
                    Total      = total,
                    Percentage = pct,
                    DailyStatus = markedDates.Select(d => new
                    {
                        Date   = d.ToString("yyyy-MM-dd"),
                        Day    = d.Day,
                        Status = attMap.TryGetValue(d, out var st) ? st : "N/A"
                    }).ToList()
                };
            }).ToList();

            return Json(new
            {
                success          = true,
                markedDates      = markedDates.Select(d => new
                {
                    date    = d.ToString("yyyy-MM-dd"),
                    day     = d.Day,
                    dayName = d.DayOfWeek.ToString().Substring(0, 3)
                }),
                rows,
                totalWorkingDays = markedDates.Count
            });
        }

        // ── Student Detail: full attendance for one student in a month ────────
        [HttpGet]
        public async Task<IActionResult> GetStudentAttendanceDetail(int studentId, int sessionId, int year, int month)
        {
            var student = await _context.TblStudents
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.StudentId, s.StudentName, s.RollNo, s.AdmissionNo })
                .FirstOrDefaultAsync();

            if (student == null)
                return Json(new { success = false, message = "Student not found" });

            var from = new DateOnly(year, month, 1);
            var to   = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            var records = await _context.TblStudentAttendances
                .Where(a => a.StudentId == studentId && a.SessionId == sessionId
                         && a.AttendanceDate >= from && a.AttendanceDate <= to
                         && a.IsActive == true)
                .OrderBy(a => a.AttendanceDate)
                .Select(a => new
                {
                    Date    = a.AttendanceDate.ToString("yyyy-MM-dd"),
                    Day     = a.AttendanceDate.Day,
                    DayName = a.AttendanceDate.DayOfWeek.ToString(),
                    a.Status
                })
                .ToListAsync();

            int present = records.Count(r => r.Status == "Present");
            int absent  = records.Count(r => r.Status == "Absent");
            int late    = records.Count(r => r.Status == "Late");
            int total   = records.Count;
            double pct  = total > 0 ? Math.Round((double)present / total * 100, 1) : 0;

            int streak = 0;
            foreach (var r in records.OrderByDescending(r => r.Date))
            {
                if (r.Status == "Present") streak++;
                else break;
            }

            return Json(new
            {
                success = true,
                student,
                summary = new { present, absent, late, total, percentage = pct, streak },
                records
            });
        }
    }

    public class AttendanceSaveRequest
    {
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public string Date { get; set; } = "";
        public List<AttendanceEntry> Entries { get; set; } = new();
    }

    public class AttendanceEntry
    {
        public int AttendanceId { get; set; }
        public int StudentId { get; set; }
        public string Status { get; set; } = "Present";
    }
}
