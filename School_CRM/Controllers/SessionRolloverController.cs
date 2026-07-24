using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class SessionRolloverController : Controller
    {
        private readonly LibmanagementContext _context;

        public SessionRolloverController(LibmanagementContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var uid) ? uid : 1;

        // ── PAGES ────────────────────────────────────────────────────
        public IActionResult Index()   => View();
        public IActionResult History() => View();

        // ── GET: Sessions dropdown ───────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var list = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new
                {
                    s.SessionId,
                    s.SessionName,
                    StartDate = s.StartDate.HasValue ? s.StartDate.Value.ToString("dd-MM-yyyy") : "-",
                    EndDate   = s.EndDate.HasValue   ? s.EndDate.Value.ToString("dd-MM-yyyy")   : "-"
                })
                .ToListAsync();

            return Json(list);
        }

        // ── GET: All active classes (for next-class lookup) ──────────
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var list = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassId)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();

            return Json(list);
        }

        // ── GET: ClassSections for Step 2 ────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetClassSections(int sourceSessionId)
        {
            var list = await _context.TblClassSections
                .Include(x => x.Class)
                .Include(x => x.Section)
                .Where(x => x.SessionId == sourceSessionId && x.IsActive == true)
                .OrderBy(x => x.Class.ClassName)
                .ThenBy(x => x.Section.SectionName)
                .Select(x => new
                {
                    x.Id,
                    x.ClassId,
                    className   = x.Class.ClassName,
                    x.SectionId,
                    sectionName = x.Section.SectionName
                })
                .ToListAsync();

            return Json(new { success = true, data = list });
        }

        // ── GET: Timetable entries for Step 3 ────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetTimetableEntries(
            int sourceSessionId,
            [FromQuery] List<int> classSectionIds)
        {
            // resolve classId+sectionId pairs from selected ClassSection IDs
            var pairs = await _context.TblClassSections
                .Where(x => classSectionIds.Contains(x.Id))
                .Select(x => new { x.ClassId, x.SectionId })
                .ToListAsync();

            var classIds   = pairs.Select(x => x.ClassId).Distinct().ToList();
            var sectionIds = pairs.Select(x => x.SectionId).Distinct().ToList();

            var entries = await _context.TblTimeTables
                .Include(tt => tt.Subject)
                .Include(tt => tt.Teacher)
                .Include(tt => tt.Period)
                .Include(tt => tt.Class)
                .Include(tt => tt.Section)
                .Where(tt => tt.SessionId == sourceSessionId
                          && tt.IsActive == true
                          && classIds.Contains(tt.ClassId)
                          && sectionIds.Contains(tt.SectionId))
                .OrderBy(tt => tt.Class.ClassName)
                .ThenBy(tt => tt.Section.SectionName)
                .ThenBy(tt => tt.DayOfWeek)
                .ThenBy(tt => tt.Period.SequenceNo)
                .Select(tt => new
                {
                    tt.TimeTableId,
                    tt.ClassId,
                    className   = tt.Class.ClassName,
                    tt.SectionId,
                    sectionName = tt.Section.SectionName,
                    tt.DayOfWeek,
                    dayName     = tt.DayOfWeek == 1 ? "Monday"
                                : tt.DayOfWeek == 2 ? "Tuesday"
                                : tt.DayOfWeek == 3 ? "Wednesday"
                                : tt.DayOfWeek == 4 ? "Thursday"
                                : tt.DayOfWeek == 5 ? "Friday"
                                                    : "Saturday",
                    tt.PeriodId,
                    periodName  = tt.Period.PeriodName,
                    tt.SubjectId,
                    subjectName = tt.Subject.SubjectName,
                    tt.TeacherId,
                    teacherName = tt.Teacher.TeacherName
                })
                .ToListAsync();

            return Json(new { success = true, data = entries });
        }

        // ── GET: Students for Step 4 ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetStudentsForPromotion(int sourceSessionId)
        {
            var students = await _context.TblStudentSessions
                .Include(s => s.Student)
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Where(s => s.SessionId == sourceSessionId && s.IsActive == true)
                .OrderBy(s => s.Class!.ClassName)
                .ThenBy(s => s.Section!.SectionName)
                .ThenBy(s => s.Student!.StudentName)
                .Select(s => new
                {
                    enrollmentId = s.Id,
                    s.StudentId,
                    studentName  = s.Student!.StudentName,
                    rollNo       = s.Student.RollNo,
                    s.ClassId,
                    className    = s.Class!.ClassName,
                    s.SectionId,
                    sectionName  = s.Section!.SectionName
                })
                .ToListAsync();

            return Json(new { success = true, data = students });
        }

        // ── GET: Next class in sequence ──────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetNextClass(int currentClassId)
        {
            var classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassId)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();

            var idx = classes.FindIndex(c => c.ClassId == currentClassId);
            if (idx < 0)
                return Json(new { success = false, message = "Class not found" });

            if (idx == classes.Count - 1)
                return Json(new { success = true, isHighest = true, nextClass = (object?)null });

            return Json(new { success = true, isHighest = false, nextClass = classes[idx + 1] });
        }

        // ── GET: Available sections in target session for a class ─────
        [HttpGet]
        public async Task<IActionResult> GetAvailableSections(int classId, int targetSessionId)
        {
            var sections = await _context.TblClassSections
                .Include(x => x.Section)
                .Where(x => x.ClassId      == classId
                         && x.SessionId    == targetSessionId
                         && x.IsActive     == true)
                .Select(x => new { x.SectionId, sectionName = x.Section.SectionName })
                .ToListAsync();

            return Json(new { success = true, data = sections });
        }

        // ── GET: Per-student promotion history ───────────────────────
        [HttpGet]
        public async Task<IActionResult> GetStudentHistory(int studentId)
        {
            var student = await _context.TblStudents
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.StudentName, s.RollNo })
                .FirstOrDefaultAsync();

            var sessions = await _context.TblStudentSessions
                .Include(s => s.Session)
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Where(s => s.StudentId == studentId)
                .OrderBy(s => s.Session!.SessionId)
                .Select(s => new
                {
                    sessionName      = s.Session!.SessionName,
                    className        = s.Class!.ClassName,
                    sectionName      = s.Section!.SectionName,
                    promotionAction  = s.PromotionAction ?? "Manual",
                    retentionReason  = s.RetentionReason,
                    retentionRemarks = s.RetentionRemarks,
                    isActive         = s.IsActive
                })
                .ToListAsync();

            var exits = await _context.TblStudentExits
                .Include(e => e.Session)
                .Where(e => e.StudentId == studentId && e.IsActive)
                .OrderBy(e => e.SessionId)
                .Select(e => new
                {
                    sessionName = e.Session.SessionName,
                    e.ExitReason,
                    exitDate    = e.ExitDate.ToString("dd-MM-yyyy"),
                    e.Remarks
                })
                .ToListAsync();

            return Json(new { success = true, student, sessions, exits });
        }

        // ── GET: Rollover history log list ───────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var logs = await _context.TblPromotionLogs
                .Include(l => l.SourceSession)
                .Include(l => l.TargetSession)
                .OrderByDescending(l => l.ExecutedAt)
                .Select(l => new
                {
                    l.Id,
                    sourceSession          = l.SourceSession.SessionName,
                    targetSession          = l.TargetSession.SessionName,
                    l.ClassSectionCreatedCount,
                    l.TimetableCreatedCount,
                    l.PromotedCount,
                    l.FailedCount,
                    l.RetainedOtherCount,
                    l.PassoutCount,
                    l.LeftSchoolCount,
                    executedAt             = l.ExecutedAt.ToString("dd-MM-yyyy HH:mm"),
                    l.Status
                })
                .ToListAsync();

            return Json(new { data = logs });
        }

        // ── POST: Main Execute ────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Execute([FromBody] RolloverExecuteDto dto)
        {
            if (dto == null)
                return Json(new { success = false, message = "Invalid request." });

            if (dto.SourceSessionId == dto.TargetSessionId)
                return Json(new { success = false, message = "Source aur Target session alag hone chahiye." });

            var targetOk = await _context.TblAcademicSessions
                .AnyAsync(s => s.SessionId == dto.TargetSessionId && s.IsActive == true);
            if (!targetOk)
                return Json(new { success = false, message = "Target session active nahi hai." });

            // build the log record now (counts filled later)
            var log = new TblPromotionLog
            {
                SourceSessionId  = dto.SourceSessionId,
                TargetSessionId  = dto.TargetSessionId,
                ExecutedByUserId = CurrentUserId,
                ExecutedAt       = DateTime.UtcNow,
                Status           = "Completed"
            };

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // ── Stage 1: ClassSection Rollover ────────────────────
                int csSaved = 0;
                if (dto.SelectedClassSectionIds?.Any() == true)
                {
                    var srcList = await _context.TblClassSections
                        .Where(x => dto.SelectedClassSectionIds.Contains(x.Id)
                                 && x.IsActive == true)
                        .ToListAsync();

                    foreach (var src in srcList)
                    {
                        bool dup = await _context.TblClassSections.AnyAsync(x =>
                            x.ClassId   == src.ClassId
                         && x.SectionId == src.SectionId
                         && x.SessionId == dto.TargetSessionId
                         && x.IsActive  == true);

                        if (!dup)
                        {
                            _context.TblClassSections.Add(new TblClassSection
                            {
                                ClassId     = src.ClassId,
                                SectionId   = src.SectionId,
                                SessionId   = dto.TargetSessionId,
                                IsActive    = true,
                                CreatedBy   = CurrentUserId,
                                CreatedDate = DateTime.Now
                                // PromotionLogId set after log saved
                            });
                            csSaved++;
                        }
                    }
                }

                // ── Stage 2: Timetable Rollover ───────────────────────
                int ttSaved = 0;
                if (dto.SelectedTimetableIds?.Any() == true)
                {
                    var srcList = await _context.TblTimeTables
                        .Where(x => dto.SelectedTimetableIds.Contains(x.TimeTableId)
                                 && x.IsActive == true)
                        .ToListAsync();

                    foreach (var src in srcList)
                    {
                        bool dup = await _context.TblTimeTables.AnyAsync(x =>
                            x.SessionId == dto.TargetSessionId
                         && x.ClassId   == src.ClassId
                         && x.SectionId == src.SectionId
                         && x.PeriodId  == src.PeriodId
                         && x.DayOfWeek == src.DayOfWeek
                         && x.IsActive  == true);

                        if (!dup)
                        {
                            _context.TblTimeTables.Add(new TblTimeTable
                            {
                                SessionId   = dto.TargetSessionId,
                                ClassId     = src.ClassId,
                                SectionId   = src.SectionId,
                                PeriodId    = src.PeriodId,
                                DayOfWeek   = src.DayOfWeek,
                                TeacherId   = src.TeacherId,
                                SubjectId   = src.SubjectId,
                                IsActive    = true,
                                CreatedBy   = CurrentUserId,
                                CreatedDate = DateTime.Now
                            });
                            ttSaved++;
                        }
                    }
                }

                // ── Save log first to get log.Id ──────────────────────
                log.ClassSectionCreatedCount = csSaved;
                log.TimetableCreatedCount    = ttSaved;
                _context.TblPromotionLogs.Add(log);
                await _context.SaveChangesAsync();   // log.Id available now

                // backfill PromotionLogId on newly added ClassSection rows
                await _context.TblClassSections
                    .Where(x => x.SessionId      == dto.TargetSessionId
                             && x.PromotionLogId == null
                             && x.CreatedBy      == CurrentUserId)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(x => x.PromotionLogId, log.Id));

                // backfill PromotionLogId on newly added TimeTable rows
                await _context.TblTimeTables
                    .Where(x => x.SessionId      == dto.TargetSessionId
                             && x.PromotionLogId == null
                             && x.CreatedBy      == CurrentUserId)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(x => x.PromotionLogId, log.Id));

                // ── Stages 3-7: Student Actions ───────────────────────
                int promoted = 0, failed = 0, retainedOther = 0,
                    passout  = 0, leftSchool = 0;

                if (dto.StudentActions?.Any() == true)
                {
                    foreach (var a in dto.StudentActions)
                    {
                        var act = a.Action?.ToLower() ?? "";

                        switch (act)
                        {
                            // ── PROMOTE ──────────────────────────────
                            case "promote":
                            {
                                bool dup = await _context.TblStudentSessions.AnyAsync(s =>
                                    s.StudentId == a.StudentId
                                 && s.SessionId == dto.TargetSessionId
                                 && s.IsActive  == true);
                                if (!dup)
                                {
                                    _context.TblStudentSessions.Add(new TblStudentSession
                                    {
                                        StudentId        = a.StudentId,
                                        SessionId        = dto.TargetSessionId,
                                        ClassId          = a.TargetClassId,
                                        SectionId        = a.TargetSectionId,
                                        PromotionAction  = "Promoted",
                                        RetentionReason  = null,
                                        RetentionRemarks = null,
                                        PromotionLogId   = log.Id,
                                        IsActive         = true,
                                        CreatedBy        = CurrentUserId,
                                        CreatedDate      = DateTime.Now
                                    });
                                    promoted++;
                                }
                                break;
                            }

                            // ── FAILED (same class retain) ────────────
                            case "failed":
                            {
                                bool dup = await _context.TblStudentSessions.AnyAsync(s =>
                                    s.StudentId == a.StudentId
                                 && s.SessionId == dto.TargetSessionId
                                 && s.IsActive  == true);
                                if (!dup)
                                {
                                    _context.TblStudentSessions.Add(new TblStudentSession
                                    {
                                        StudentId        = a.StudentId,
                                        SessionId        = dto.TargetSessionId,
                                        ClassId          = a.TargetClassId,
                                        SectionId        = a.TargetSectionId,
                                        PromotionAction  = "Failed",
                                        RetentionReason  = "Failed",
                                        RetentionRemarks = a.RetentionRemarks,
                                        PromotionLogId   = log.Id,
                                        IsActive         = true,
                                        CreatedBy        = CurrentUserId,
                                        CreatedDate      = DateTime.Now
                                    });
                                    failed++;
                                }
                                break;
                            }

                            // ── RETAINED OTHER ────────────────────────
                            case "retained":
                            {
                                bool dup = await _context.TblStudentSessions.AnyAsync(s =>
                                    s.StudentId == a.StudentId
                                 && s.SessionId == dto.TargetSessionId
                                 && s.IsActive  == true);
                                if (!dup)
                                {
                                    _context.TblStudentSessions.Add(new TblStudentSession
                                    {
                                        StudentId        = a.StudentId,
                                        SessionId        = dto.TargetSessionId,
                                        ClassId          = a.TargetClassId,
                                        SectionId        = a.TargetSectionId,
                                        PromotionAction  = "Retained",
                                        RetentionReason  = "Other",
                                        RetentionRemarks = a.RetentionRemarks,
                                        PromotionLogId   = log.Id,
                                        IsActive         = true,
                                        CreatedBy        = CurrentUserId,
                                        CreatedDate      = DateTime.Now
                                    });
                                    retainedOther++;
                                }
                                break;
                            }

                            // ── PASSOUT ───────────────────────────────
                            case "passout":
                            {
                                var src = await _context.TblStudentSessions
                                    .FirstOrDefaultAsync(s =>
                                        s.StudentId == a.StudentId
                                     && s.SessionId == dto.SourceSessionId
                                     && s.IsActive  == true);
                                if (src != null)
                                {
                                    src.IsActive    = false;
                                    src.UpdatedDate = DateTime.Now;
                                    src.UpdatedBy   = CurrentUserId;
                                }
                                passout++;
                                break;
                            }

                            // ── LEFT SCHOOL ───────────────────────────
                            case "leftschool":
                            {
                                var src = await _context.TblStudentSessions
                                    .FirstOrDefaultAsync(s =>
                                        s.StudentId == a.StudentId
                                     && s.SessionId == dto.SourceSessionId
                                     && s.IsActive  == true);
                                if (src != null)
                                {
                                    src.IsActive    = false;
                                    src.UpdatedDate = DateTime.Now;
                                    src.UpdatedBy   = CurrentUserId;
                                }

                                _context.TblStudentExits.Add(new TblStudentExit
                                {
                                    StudentId        = a.StudentId,
                                    SessionId        = dto.SourceSessionId,
                                    ExitReason       = a.ExitReason ?? "Other",
                                    ExitDate         = a.ExitDate.HasValue
                                                        ? DateOnly.FromDateTime(a.ExitDate.Value)
                                                        : DateOnly.FromDateTime(DateTime.Today),
                                    Remarks          = a.ExitRemarks,
                                    PromotionLogId   = log.Id,
                                    IsActive         = true,
                                    RecordedByUserId = CurrentUserId,
                                    RecordedAt       = DateTime.Now
                                });
                                leftSchool++;
                                break;
                            }
                        }
                    }
                }

                // update counts on log
                log.PromotedCount      = promoted;
                log.FailedCount        = failed;
                log.RetainedOtherCount = retainedOther;
                log.PassoutCount       = passout;
                log.LeftSchoolCount    = leftSchool;

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Promotion successfully completed!",
                    logId   = log.Id,
                    summary = new
                    {
                        classSectionCreated = csSaved,
                        timetableCreated    = ttSaved,
                        promoted,
                        failed,
                        retainedOther,
                        passout,
                        leftSchool
                    }
                });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Json(new { success = false, message = "Execution failed: " + ex.Message });
            }
        }

        // ── POST: Rollback ────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Rollback([FromBody] int logId)
        {
            var log = await _context.TblPromotionLogs.FindAsync(logId);
            if (log == null)
                return Json(new { success = false, message = "Log record not found." });
            if (log.Status == "RolledBack")
                return Json(new { success = false, message = "Yeh log already RolledBack hai." });

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // soft-delete ClassSections created by this run
                await _context.TblClassSections
                    .Where(x => x.PromotionLogId == logId && x.IsActive == true)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsActive,    false)
                        .SetProperty(x => x.UpdatedDate, DateTime.Now)
                        .SetProperty(x => x.UpdatedBy,   CurrentUserId));

                // soft-delete TimeTables created by this run
                await _context.TblTimeTables
                    .Where(x => x.PromotionLogId == logId && x.IsActive == true)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsActive,    false)
                        .SetProperty(x => x.UpdatedDate, DateTime.Now)
                        .SetProperty(x => x.UpdatedBy,   CurrentUserId));

                // soft-delete StudentSessions created by this run
                await _context.TblStudentSessions
                    .Where(x => x.PromotionLogId == logId && x.IsActive == true)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.IsActive,    false)
                        .SetProperty(x => x.UpdatedDate, DateTime.Now)
                        .SetProperty(x => x.UpdatedBy,   CurrentUserId));

                // restore source-session StudentSession for Left School students
                var exitStudentIds = await _context.TblStudentExits
                    .Where(e => e.PromotionLogId == logId)
                    .Select(e => e.StudentId)
                    .ToListAsync();

                if (exitStudentIds.Any())
                {
                    await _context.TblStudentSessions
                        .Where(s => exitStudentIds.Contains((int)s.StudentId!)
                                 && s.SessionId == log.SourceSessionId
                                 && s.IsActive  == false)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(x => x.IsActive,    true)
                            .SetProperty(x => x.UpdatedDate, DateTime.Now));
                }

                // soft-delete TblStudentExit records from this run
                await _context.TblStudentExits
                    .Where(e => e.PromotionLogId == logId && e.IsActive)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(x => x.IsActive, false));

                // update log status
                log.Status             = "RolledBack";
                log.RolledBackByUserId = CurrentUserId;
                log.RolledBackAt       = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return Json(new { success = true, message = "Rollback successfully completed!" });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Json(new { success = false, message = "Rollback failed: " + ex.Message });
            }
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────
    public class RolloverExecuteDto
    {
        public int SourceSessionId { get; set; }
        public int TargetSessionId { get; set; }
        public List<int>? SelectedClassSectionIds { get; set; }
        public List<int>? SelectedTimetableIds { get; set; }
        public List<StudentActionDto>? StudentActions { get; set; }
    }

    public class StudentActionDto
    {
        public int     StudentId         { get; set; }
        public string? Action            { get; set; }  // promote|failed|retained|passout|leftschool
        public int?    TargetClassId     { get; set; }
        public int?    TargetSectionId   { get; set; }
        public string? RetentionRemarks  { get; set; }
        public string? ExitReason        { get; set; }
        public DateTime? ExitDate        { get; set; }
        public string? ExitRemarks       { get; set; }
    }
}
