using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class AcademicDashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public AcademicDashboardController(LibmanagementContext context)
            => _context = context;

        // ── helpers ──────────────────────────────────────────────────────────
        private string RoleName => Request.Cookies["roleName"] ?? "";
        private int EntityId => int.TryParse(Request.Cookies["EntityId"], out var v) ? v : 0;
        private bool IsSuperAdminOrPrincipal =>
            RoleName.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
            RoleName.Equals("Admin",      StringComparison.OrdinalIgnoreCase) ||
            RoleName.Equals("Principal",  StringComparison.OrdinalIgnoreCase);

        // ─────────────────────────────────────────────────────────────────────
        //  ROUTING: redirect to role-specific dashboard
        // ─────────────────────────────────────────────────────────────────────
        public IActionResult Index()
        {
            return RoleName.ToLower() switch
            {
                "teacher"   => RedirectToAction("Teacher"),
                "student"   => RedirectToAction("Student"),
                _           => RedirectToAction("Admin")
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ADMIN / SUPERADMIN / PRINCIPAL DASHBOARD
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Admin()
        {
            ViewBag.Sessions  = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Classes   = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.RoleName  = RoleName;
            ViewBag.UserName  = Request.Cookies["EntityName"] ?? Request.Cookies["userName"] ?? "Admin";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAdminStats(int sessionId = 0)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                // Active session fallback
                if (sessionId == 0)
                    sessionId = await _context.TblAcademicSessions
                        .Where(s => s.IsActive == true)
                        .Select(s => s.SessionId)
                        .FirstOrDefaultAsync();

                // ── Counts ────────────────────────────────────────────────────
                var totalStudents  = await _context.TblStudentSessions.CountAsync(ss => ss.SessionId == sessionId && ss.IsActive == true);
                var totalTeachers  = await _context.TblTeachers.CountAsync(t => t.IsActive == true);
                var totalClasses   = await _context.TblClasses.CountAsync(c => c.IsActive == true);
                var totalSubjects  = await _context.TblSubjects.CountAsync(s => s.IsActive == true);
                var totalExams     = await _context.TblExams.CountAsync(e => e.SessionId == sessionId && e.IsActive == true);
                var totalAssignments = await _context.TblAssignments.CountAsync(a => a.SessionId == sessionId && a.IsActive == true);
                var totalAnnouncements = await _context.TblAnnouncements.CountAsync(a => a.IsActive == true);

                // ── Today Attendance ──────────────────────────────────────────
                var todayAtt = await _context.TblStudentAttendances
                    .Where(a => a.AttendanceDate == today && a.SessionId == sessionId && a.IsActive == true)
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                int presentToday = todayAtt.FirstOrDefault(x => x.Status == "Present")?.Count ?? 0;
                int absentToday  = todayAtt.FirstOrDefault(x => x.Status == "Absent")?.Count  ?? 0;
                int lateToday    = todayAtt.FirstOrDefault(x => x.Status == "Late")?.Count    ?? 0;

                // ── Attendance last 7 days (line chart) ───────────────────────
                var from7 = today.AddDays(-6);
                var weekAtt = await _context.TblStudentAttendances
                    .Where(a => a.AttendanceDate >= from7 && a.AttendanceDate <= today
                             && a.SessionId == sessionId && a.IsActive == true)
                    .GroupBy(a => new { a.AttendanceDate, a.Status })
                    .Select(g => new { g.Key.AttendanceDate, g.Key.Status, Count = g.Count() })
                    .ToListAsync();

                var attendanceTrend = Enumerable.Range(0, 7).Select(i =>
                {
                    var d = from7.AddDays(i);
                    return new
                    {
                        date    = d.ToString("dd MMM"),
                        present = weekAtt.Where(x => x.AttendanceDate == d && x.Status == "Present").Sum(x => x.Count),
                        absent  = weekAtt.Where(x => x.AttendanceDate == d && x.Status == "Absent").Sum(x => x.Count),
                        late    = weekAtt.Where(x => x.AttendanceDate == d && x.Status == "Late").Sum(x => x.Count)
                    };
                }).ToList();

                // ── Class-wise student count (bar chart) ──────────────────────
                var classWise = await _context.TblStudentSessions
                    .Where(ss => ss.SessionId == sessionId && ss.IsActive == true && ss.ClassId != null)
                    .Include(ss => ss.Class)
                    .GroupBy(ss => new { ss.ClassId, ss.Class!.ClassName })
                    .Select(g => new { className = g.Key.ClassName, count = g.Count() })
                    .OrderBy(x => x.className)
                    .ToListAsync();

                // ── Exam results summary (pie chart) ──────────────────────────
                var reportCards = await _context.TblReportCards
                    .Where(rc => rc.SessionId == sessionId && rc.IsActive == true)
                    .Include(rc => rc.Grade)
                    .GroupBy(rc => rc.Grade!.GradeName)
                    .Select(g => new { grade = g.Key, count = g.Count() })
                    .ToListAsync();

                // ── Recent announcements ──────────────────────────────────────
                var announcements = await _context.TblAnnouncements
                    .Where(a => a.IsActive == true)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(5)
                    .Select(a => new { a.Title, a.Message, a.IsGlobal, date = a.CreatedDate!.Value.ToString("dd MMM yyyy") })
                    .ToListAsync();

                // ── Upcoming assignments ──────────────────────────────────────
                var upcomingAssignments = await _context.TblAssignments
                    .Where(a => a.SessionId == sessionId && a.IsActive == true && a.DueDate >= today)
                    .Include(a => a.Class).Include(a => a.Subject).Include(a => a.Teacher)
                    .OrderBy(a => a.DueDate)
                    .Take(5)
                    .Select(a => new
                    {
                        a.Title,
                        ClassName   = a.Class.ClassName,
                        SubjectName = a.Subject.SubjectName,
                        TeacherName = a.Teacher.TeacherName,
                        DueDate     = a.DueDate.ToString("dd MMM yyyy")
                    }).ToListAsync();

                // ── Fee collection summary ────────────────────────────────────
                var feeStats = await _context.TblFeeCollections
                    .Where(f => f.IsActive == true)
                    .GroupBy(f => 1)
                    .Select(g => new
                    {
                        totalCollected = g.Sum(f => f.PaidAmount ?? 0),
                        totalCount     = g.Count()
                    }).FirstOrDefaultAsync();

                return Json(new
                {
                    totalStudents, totalTeachers, totalClasses, totalSubjects,
                    totalExams, totalAssignments, totalAnnouncements,
                    presentToday, absentToday, lateToday,
                    attendanceTrend, classWise, reportCards,
                    announcements, upcomingAssignments,
                    totalFeeCollected = feeStats?.totalCollected ?? 0,
                    todayDate = today.ToString("dddd, dd MMMM yyyy")
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TEACHER DASHBOARD
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Teacher()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.TeacherId = EntityId;
            ViewBag.UserName  = Request.Cookies["EntityName"] ?? "Teacher";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTeacherStats(int teacherId = 0, int sessionId = 0)
        {
            try
            {
                if (teacherId == 0) teacherId = EntityId;
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (sessionId == 0)
                    sessionId = await _context.TblAcademicSessions
                        .Where(s => s.IsActive == true)
                        .Select(s => s.SessionId)
                        .FirstOrDefaultAsync();

                // ── My assignments ────────────────────────────────────────────
                var myAssignments = await _context.TblAssignments
                    .Where(a => a.TeacherId == teacherId && a.SessionId == sessionId && a.IsActive == true)
                    .Include(a => a.Class).Include(a => a.Section).Include(a => a.Subject)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(10)
                    .Select(a => new
                    {
                        a.AssignmentId, a.Title,
                        ClassName   = a.Class.ClassName,
                        SectionName = a.Section.SectionName,
                        SubjectName = a.Subject.SubjectName,
                        DueDate     = a.DueDate.ToString("dd MMM yyyy"),
                        IsOverdue   = a.DueDate < today
                    }).ToListAsync();

                int totalAssignments  = myAssignments.Count;
                int overdueAssignments = myAssignments.Count(a => a.IsOverdue);

                // ── My classes (teacher assignment) ───────────────────────────
                var myClasses = await _context.TblTeacherAssignments
                    .Where(ta => ta.TeacherId == teacherId && ta.SessionId == sessionId && ta.IsActive == true)
                    .Include(ta => ta.Class).Include(ta => ta.Section).Include(ta => ta.Subject)
                    .Select(ta => new
                    {
                        ClassName   = ta.Class.ClassName,
                        SectionName = ta.Section.SectionName,
                        SubjectName = ta.Subject.SubjectName
                    }).Distinct().ToListAsync();

                // ── Today's timetable ─────────────────────────────────────────
                byte todayDow = (byte)((int)DateTime.Today.DayOfWeek == 0 ? 7 : (int)DateTime.Today.DayOfWeek);
                var todayTT = await _context.TblTimeTables
                    .Where(tt => tt.TeacherId == teacherId && tt.SessionId == sessionId
                              && tt.DayOfWeek == todayDow && tt.IsActive == true)
                    .Include(tt => tt.Period).Include(tt => tt.Class)
                    .Include(tt => tt.Section).Include(tt => tt.Subject)
                    .OrderBy(tt => tt.Period.SequenceNo)
                    .Select(tt => new
                    {
                        PeriodName  = tt.Period.PeriodName,
                        StartTime   = tt.Period.StartTime.ToString("hh\\:mm"),
                        EndTime     = tt.Period.EndTime.ToString("hh\\:mm"),
                        ClassName   = tt.Class.ClassName,
                        SectionName = tt.Section.SectionName,
                        SubjectName = tt.Subject.SubjectName
                    }).ToListAsync();

                // ── Weekly timetable ──────────────────────────────────────────
                var weeklyTT = await _context.TblTimeTables
                    .Where(tt => tt.TeacherId == teacherId && tt.SessionId == sessionId && tt.IsActive == true)
                    .Include(tt => tt.Period).Include(tt => tt.Class)
                    .Include(tt => tt.Section).Include(tt => tt.Subject)
                    .Select(tt => new
                    {
                        tt.DayOfWeek,
                        PeriodName  = tt.Period.PeriodName,
                        SeqNo       = tt.Period.SequenceNo,
                        StartTime   = tt.Period.StartTime.ToString("hh\\:mm"),
                        EndTime     = tt.Period.EndTime.ToString("hh\\:mm"),
                        ClassName   = tt.Class.ClassName,
                        SectionName = tt.Section.SectionName,
                        SubjectName = tt.Subject.SubjectName
                    }).OrderBy(tt => tt.DayOfWeek).ThenBy(tt => tt.SeqNo)
                    .ToListAsync();

                // ── Attendance I took today ───────────────────────────────────
                var attTakenClasses = await _context.TblStudentAttendances
                    .Where(a => a.AttendanceDate == today && a.SessionId == sessionId && a.IsActive == true)
                    .Select(a => new { a.ClassId, a.SectionId })
                    .Distinct().CountAsync();

                // ── My study materials ────────────────────────────────────────
                var myMaterials = await _context.TblStudyMaterials
                    .Where(m => m.TeacherId == teacherId && m.IsActive == true)
                    .Include(m => m.Subject).Include(m => m.Class)
                    .OrderByDescending(m => m.CreatedDate)
                    .Take(5)
                    .Select(m => new
                    {
                        m.Title,
                        SubjectName = m.Subject.SubjectName,
                        ClassName   = m.Class.ClassName,
                        HasFile     = !string.IsNullOrEmpty(m.FilePath),
                        date        = m.CreatedDate!.Value.ToString("dd MMM")
                    }).ToListAsync();

                // ── Custom tests ──────────────────────────────────────────────
                var myTests = await _context.TblCustomTests
                    .Where(t => t.TeacherId == teacherId && t.IsActive == true)
                    .Include(t => t.Subject).Include(t => t.Class)
                    .OrderByDescending(t => t.TestDate)
                    .Take(5)
                    .Select(t => new
                    {
                        t.TestName,
                        SubjectName = t.Subject.SubjectName,
                        ClassName   = t.Class.ClassName,
                        TestDate    = t.TestDate.ToString("dd MMM yyyy"),
                        MaxMarks    = t.MaxMarks.ToString("0")
                    }).ToListAsync();

                // ── Attendance summary per class (bar) ────────────────────────
                var attSummary = await _context.TblStudentAttendances
                    .Where(a => a.AttendanceDate == today && a.SessionId == sessionId && a.IsActive == true)
                    .Include(a => a.Class)
                    .GroupBy(a => new { a.ClassId, a.Class.ClassName, a.Status })
                    .Select(g => new { g.Key.ClassName, g.Key.Status, Count = g.Count() })
                    .ToListAsync();

                return Json(new
                {
                    totalAssignments, overdueAssignments,
                    totalClasses = myClasses.Count,
                    attTakenClasses,
                    totalMaterials = myMaterials.Count,
                    myAssignments, myClasses, todayTT, weeklyTT,
                    myMaterials, myTests, attSummary,
                    todayDate = today.ToString("dddd, dd MMMM yyyy"),
                    todayDayName = DateTime.Today.ToString("dddd")
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  STUDENT DASHBOARD
        // ─────────────────────────────────────────────────────────────────────
        public async Task<IActionResult> Student()
        {
            ViewBag.Sessions  = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.StudentId = EntityId;
            ViewBag.UserName  = Request.Cookies["EntityName"] ?? "Student";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentStats(int studentId = 0, int sessionId = 0)
        {
            try
            {
                if (studentId == 0) studentId = EntityId;
                var today = DateOnly.FromDateTime(DateTime.Today);

                if (sessionId == 0)
                    sessionId = await _context.TblAcademicSessions
                        .Where(s => s.IsActive == true)
                        .Select(s => s.SessionId)
                        .FirstOrDefaultAsync();

                // ── My session info ───────────────────────────────────────────
                var mySession = await _context.TblStudentSessions
                    .Where(ss => ss.StudentId == studentId && ss.SessionId == sessionId && ss.IsActive == true)
                    .Include(ss => ss.Class).Include(ss => ss.Section).Include(ss => ss.Session)
                    .Select(ss => new
                    {
                        ClassName   = ss.Class != null ? ss.Class.ClassName : "-",
                        SectionName = ss.Section != null ? ss.Section.SectionName : "-",
                        SessionName = ss.Session != null ? ss.Session.SessionName : "-",
                        ss.ClassId, ss.SectionId
                    }).FirstOrDefaultAsync();

                // ── Attendance summary ────────────────────────────────────────
                var myAtt = await _context.TblStudentAttendances
                    .Where(a => a.StudentId == studentId && a.SessionId == sessionId && a.IsActive == true)
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                int totalDays   = myAtt.Sum(x => x.Count);
                int presentDays = myAtt.FirstOrDefault(x => x.Status == "Present")?.Count ?? 0;
                int absentDays  = myAtt.FirstOrDefault(x => x.Status == "Absent")?.Count  ?? 0;
                int lateDays    = myAtt.FirstOrDefault(x => x.Status == "Late")?.Count    ?? 0;
                double attPct   = totalDays > 0 ? Math.Round((double)presentDays / totalDays * 100, 1) : 0;

                // ── Attendance last 30 days (calendar data) ───────────────────
                var from30 = today.AddDays(-29);
                var recentAtt = await _context.TblStudentAttendances
                    .Where(a => a.StudentId == studentId && a.AttendanceDate >= from30
                             && a.AttendanceDate <= today && a.IsActive == true)
                    .Select(a => new { date = a.AttendanceDate.ToString("yyyy-MM-dd"), a.Status })
                    .ToListAsync();

                // ── My report card ────────────────────────────────────────────
                var reportCard = await _context.TblReportCards
                    .Where(rc => rc.StudentId == studentId && rc.SessionId == sessionId
                              && rc.IsActive == true && rc.IsPublished == true)
                    .Include(rc => rc.Grade)
                    .Include(rc => rc.TblReportCardSubjects).ThenInclude(rcs => rcs.Subject)
                    .Include(rc => rc.TblReportCardSubjects).ThenInclude(rcs => rcs.Grade)
                    .Select(rc => new
                    {
                        rc.TotalMarks, rc.ObtainedMarks, rc.Percentage,
                        GradeName = rc.Grade!.GradeName,
                        Remark    = rc.Grade.Remark,
                        rc.Rank,
                        Subjects  = rc.TblReportCardSubjects.Select(s => new
                        {
                            SubjectName   = s.Subject!.SubjectName,
                            s.MaxMarks, s.ObtainedMarks, s.Percentage,
                            GradeName     = s.Grade!.GradeName
                        }).ToList()
                    }).FirstOrDefaultAsync();

                // ── Pending assignments + timetable + materials ───────────────
                int? classId   = mySession?.ClassId;
                int? sectionId = mySession?.SectionId;

                // ── Today's timetable ─────────────────────────────────────────
                byte todayDow = (byte)((int)DateTime.Today.DayOfWeek == 0 ? 7 : (int)DateTime.Today.DayOfWeek);
                List<object> todayTT;
                List<object> weeklyTT;
                List<object> pendingAssignments;
                List<object> materials;

                if (classId.HasValue)
                {
                    todayTT = (await _context.TblTimeTables
                        .Where(tt => tt.ClassId == classId && tt.SectionId == sectionId
                                  && tt.SessionId == sessionId && tt.DayOfWeek == todayDow && tt.IsActive == true)
                        .Include(tt => tt.Period).Include(tt => tt.Subject).Include(tt => tt.Teacher)
                        .OrderBy(tt => tt.Period.SequenceNo)
                        .Select(tt => new
                        {
                            PeriodName  = tt.Period.PeriodName,
                            StartTime   = tt.Period.StartTime.ToString("hh\\:mm"),
                            EndTime     = tt.Period.EndTime.ToString("hh\\:mm"),
                            SubjectName = tt.Subject.SubjectName,
                            TeacherName = tt.Teacher.TeacherName,
                            IsBrake     = tt.Period.IsBrake
                        }).ToListAsync()).Cast<object>().ToList();

                    weeklyTT = (await _context.TblTimeTables
                        .Where(tt => tt.ClassId == classId && tt.SectionId == sectionId
                                  && tt.SessionId == sessionId && tt.IsActive == true)
                        .Include(tt => tt.Period).Include(tt => tt.Subject).Include(tt => tt.Teacher)
                        .Select(tt => new
                        {
                            tt.DayOfWeek,
                            PeriodName  = tt.Period.PeriodName,
                            SeqNo       = tt.Period.SequenceNo,
                            StartTime   = tt.Period.StartTime.ToString("hh\\:mm"),
                            EndTime     = tt.Period.EndTime.ToString("hh\\:mm"),
                            SubjectName = tt.Subject.SubjectName,
                            TeacherName = tt.Teacher.TeacherName,
                            IsBrake     = tt.Period.IsBrake
                        }).OrderBy(tt => tt.DayOfWeek).ThenBy(tt => tt.SeqNo)
                        .ToListAsync()).Cast<object>().ToList();

                    pendingAssignments = (await _context.TblAssignments
                        .Where(a => a.ClassId == classId && a.SectionId == sectionId
                                 && a.SessionId == sessionId && a.IsActive == true && a.DueDate >= today)
                        .Include(a => a.Subject).Include(a => a.Teacher)
                        .OrderBy(a => a.DueDate)
                        .Take(5)
                        .Select(a => new
                        {
                            a.Title,
                            SubjectName = a.Subject.SubjectName,
                            TeacherName = a.Teacher.TeacherName,
                            DueDate     = a.DueDate.ToString("dd MMM yyyy"),
                            DaysLeft    = a.DueDate.DayNumber - today.DayNumber
                        }).ToListAsync()).Cast<object>().ToList();

                    materials = (await _context.TblStudyMaterials
                        .Where(m => m.ClassId == classId && m.SectionId == sectionId && m.IsActive == true)
                        .Include(m => m.Subject).Include(m => m.Teacher)
                        .OrderByDescending(m => m.CreatedDate)
                        .Take(5)
                        .Select(m => new
                        {
                            m.Title,
                            SubjectName = m.Subject.SubjectName,
                            TeacherName = m.Teacher.TeacherName,
                            HasFile     = !string.IsNullOrEmpty(m.FilePath),
                            FilePath    = m.FilePath ?? "",
                            date        = m.CreatedDate!.Value.ToString("dd MMM")
                        }).ToListAsync()).Cast<object>().ToList();
                }
                else
                {
                    todayTT = new List<object>();
                    weeklyTT = new List<object>();
                    pendingAssignments = new List<object>();
                    materials = new List<object>();
                }

                // ── Announcements ─────────────────────────────────────────────
                var announcements = await _context.TblAnnouncements
                    .Where(a => a.IsActive == true &&
                               (a.IsGlobal == true || a.ClassId == classId))
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(5)
                    .Select(a => new { a.Title, a.Message, date = a.CreatedDate!.Value.ToString("dd MMM yyyy") })
                    .ToListAsync();

                return Json(new
                {
                    mySession,
                    totalDays, presentDays, absentDays, lateDays, attPct,
                    recentAtt, reportCard,
                    pendingAssignments, todayTT, weeklyTT,
                    announcements, materials,
                    todayDate    = today.ToString("dddd, dd MMMM yyyy"),
                    todayDayName = DateTime.Today.ToString("dddd")
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }
}
