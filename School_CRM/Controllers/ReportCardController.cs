using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ReportCardController : Controller
    {
        private readonly LibmanagementContext _context;
        public ReportCardController(LibmanagementContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int sessionId = 0, int classId = 0, int sectionId = 0)
        {
            var query = _context.TblReportCards
                .Where(rc => rc.IsActive == true)
                .Include(rc => rc.Student)
                .Include(rc => rc.Session)
                .Include(rc => rc.Class)
                .Include(rc => rc.Section)
                .Include(rc => rc.Grade)
                .AsQueryable();

            if (sessionId > 0) query = query.Where(rc => rc.SessionId == sessionId);
            if (classId > 0) query = query.Where(rc => rc.ClassId == classId);
            if (sectionId > 0) query = query.Where(rc => rc.SectionId == sectionId);

            var data = await query.Select(rc => new
            {
                rc.ReportCardId,
                StudentName = rc.Student.StudentName,
                SessionName = rc.Session.SessionName,
                ClassName = rc.Class.ClassName,
                SectionName = rc.Section.SectionName,
                TotalMarks = rc.TotalMarks.ToString("0.00"),
                ObtainedMarks = rc.ObtainedMarks.ToString("0.00"),
                Percentage = rc.Percentage.ToString("0.00") + "%",
                GradeName = rc.Grade.GradeName,
                rc.Rank,
                IsPublished = rc.IsPublished == true ? "Published" : "Draft",
                GeneratedDate = rc.GeneratedDate.HasValue ? rc.GeneratedDate.Value.ToString("dd-MM-yyyy") : "-"
            }).ToListAsync();

            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> Generate([FromBody] GenerateReportRequest request)
        {
            try
            {
                var students = await _context.TblStudentSessions
                    .Where(ss => ss.SessionId == request.SessionId && ss.ClassId == request.ClassId
                        && ss.SectionId == request.SectionId && ss.IsActive == true && ss.StudentId != null)
                    .ToListAsync();

                var exams = await _context.TblExams
                    .Where(e => e.SessionId == request.SessionId && e.IsActive == true)
                    .ToListAsync();

                var examIds = exams.Select(e => e.ExamId).ToList();

                var weightages = await _context.TblExamWeightages
                    .Where(ew => ew.SessionId == request.SessionId && ew.IsActive == true)
                    .ToListAsync();

                // Fix: filter by both ClassId AND the exams belonging to this session
                var examSubjects = await _context.TblExamSubjects
                    .Where(es => es.ClassId == request.ClassId && examIds.Contains(es.ExamId) && es.IsActive == true)
                    .ToListAsync();

                // Pre-load all marks for this class/session to avoid N+1 queries and IsActive=null misses
                var allStudentIds = students.Where(ss => ss.StudentId != null).Select(ss => ss.StudentId!.Value).ToList();
                var allMarks = await _context.TblExamMarks
                    .Where(m => examIds.Contains(m.ExamId) && allStudentIds.Contains(m.StudentId))
                    .ToListAsync();

                var grades = await _context.TblGradeMasters
                    .Where(g => g.IsActive == true)
                    .OrderByDescending(g => g.MinPercent)
                    .ToListAsync();

                int generated = 0;
                foreach (var ss in students)
                {
                    int studentId = ss.StudentId!.Value;
                    decimal totalMax = 0, totalObtained = 0;
                    var subjectBreakdown = new List<(int SubjectId, decimal Max, decimal Obtained)>();

                    foreach (var exam in exams)
                    {
                        var weight = weightages.FirstOrDefault(w => w.ExamId == exam.ExamId)?.WeightPct ?? 100m;
                        var subjects = examSubjects.Where(es => es.ExamId == exam.ExamId).ToList();

                        foreach (var es in subjects)
                        {
                            // Use pre-loaded marks (no IsActive filter — treat null as active)
                            var mark = allMarks.FirstOrDefault(m => m.ExamId == exam.ExamId
                                && m.StudentId == studentId
                                && m.SubjectId == es.SubjectId);

                            decimal obtained = mark?.IsAbsent == true ? 0 : (mark?.MarksObtained ?? 0);
                            decimal weightedMax = es.MaxMarks * weight / 100;
                            decimal weightedObtained = obtained * weight / 100;

                            totalMax += weightedMax;
                            totalObtained += weightedObtained;

                            var existing = subjectBreakdown.FirstOrDefault(sb => sb.SubjectId == es.SubjectId);
                            if (existing == default)
                                subjectBreakdown.Add((es.SubjectId, weightedMax, weightedObtained));
                            else
                            {
                                subjectBreakdown.Remove(existing);
                                subjectBreakdown.Add((es.SubjectId, existing.Max + weightedMax, existing.Obtained + weightedObtained));
                            }
                        }
                    }

                    decimal percentage = totalMax > 0 ? Math.Round(totalObtained / totalMax * 100, 2) : 0;
                    var grade = grades.FirstOrDefault(g => percentage >= g.MinPercent && percentage <= g.MaxPercent) ?? grades.LastOrDefault();
                    if (grade == null) continue;

                    string status = percentage >= 33 ? "PASS" : "FAIL";
                    string remark = percentage >= 90 ? "Outstanding performance!" : (percentage >= 75 ? "Excellent work." : (percentage >= 60 ? "Good, but can improve." : (percentage >= 33 ? "Needs more focus." : "Needs serious improvement.")));

                    var existingCard = await _context.TblReportCards
                        .FirstOrDefaultAsync(rc => rc.StudentId == studentId && rc.SessionId == request.SessionId && rc.IsActive == true);

                    int reportCardId;
                    if (existingCard == null)
                    {
                        var card = new TblReportCard
                        {
                            StudentId = studentId,
                            SessionId = request.SessionId,
                            ClassId = request.ClassId,
                            SectionId = request.SectionId,
                            TotalMarks = totalMax,
                            ObtainedMarks = totalObtained,
                            Percentage = percentage,
                            GradeId = grade.GradeId,
                            ResultStatus = status,
                            TeacherRemark = remark,
                            VerificationCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper(),
                            IsPublished = false,
                            GeneratedDate = DateTime.Now,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        };
                        _context.TblReportCards.Add(card);
                        await _context.SaveChangesAsync();
                        reportCardId = card.ReportCardId;
                    }
                    else
                    {
                        existingCard.TotalMarks = totalMax;
                        existingCard.ObtainedMarks = totalObtained;
                        existingCard.Percentage = percentage;
                        existingCard.GradeId = grade.GradeId;
                        existingCard.ResultStatus = status;
                        existingCard.TeacherRemark = remark;
                        if(string.IsNullOrEmpty(existingCard.VerificationCode)) existingCard.VerificationCode = Guid.NewGuid().ToString("N").Substring(0, 10).ToUpper();
                        existingCard.GeneratedDate = DateTime.Now;
                        existingCard.UpdatedDate = DateTime.Now;
                        reportCardId = existingCard!.ReportCardId;

                        var oldSubjects = _context.TblReportCardSubjects.Where(rcs => rcs.ReportCardId == reportCardId);
                        _context.TblReportCardSubjects.RemoveRange(oldSubjects);
                    }

                    foreach (var (subjectId, max, obtained) in subjectBreakdown)
                    {
                        decimal subPct = max > 0 ? Math.Round(obtained / max * 100, 2) : 0;
                        var subGrade = grades.FirstOrDefault(g => subPct >= g.MinPercent && subPct <= g.MaxPercent) ?? grades.LastOrDefault();
                        _context.TblReportCardSubjects.Add(new TblReportCardSubject
                        {
                            ReportCardId = reportCardId,
                            SubjectId = subjectId,
                            MaxMarks = max,
                            ObtainedMarks = obtained,
                            Percentage = subPct,
                            GradeId = subGrade!.GradeId,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        });
                    }

                    generated++;
                }

                await _context.SaveChangesAsync();

                // Update ranks
                var cards = await _context.TblReportCards
                    .Where(rc => rc.SessionId == request.SessionId && rc.ClassId == request.ClassId && rc.SectionId == request.SectionId && rc.IsActive == true)
                    .OrderByDescending(rc => rc.Percentage)
                    .ToListAsync();
                for (int i = 0; i < cards.Count; i++) cards[i].Rank = i + 1;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"Report cards generated for {generated} students!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Publish(int id)
        {
            var item = await _context.TblReportCards.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsPublished = true;
            item.PublishedDate = DateTime.Now;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Report card published!" });
        }

        [HttpPost]
        public async Task<IActionResult> BulkPublish([FromBody] BulkPublishRequest request)
        {
            if(request.SessionId == 0 || request.ClassId == 0)
                return Json(new { success = false, message = "Invalid parameters." });

            var query = _context.TblReportCards.Where(rc => rc.SessionId == request.SessionId && rc.ClassId == request.ClassId && rc.IsActive == true);
            if(request.SectionId > 0)
                query = query.Where(rc => rc.SectionId == request.SectionId);

            var reportCards = await query.ToListAsync();
            if(!reportCards.Any())
                return Json(new { success = false, message = "No report cards found to publish." });

            int count = 0;
            foreach(var rc in reportCards)
            {
                if(rc.IsPublished != request.IsPublish)
                {
                    rc.IsPublished = request.IsPublish;
                    rc.PublishedDate = request.IsPublish ? DateTime.Now : null;
                    rc.UpdatedDate = DateTime.Now;
                    count++;
                }
            }
            
            if(count > 0) await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"{count} Report Cards {(request.IsPublish ? "Published" : "Unpublished")} successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> ViewCard(int id)
        {
            var card = await _context.TblReportCards
                .Include(rc => rc.Student)
                .Include(rc => rc.Session)
                .Include(rc => rc.Class)
                .Include(rc => rc.Section)
                .Include(rc => rc.Grade)
                .Include(rc => rc.TblReportCardSubjects).ThenInclude(rcs => rcs.Subject)
                .Include(rc => rc.TblReportCardSubjects).ThenInclude(rcs => rcs.Grade)
                .FirstOrDefaultAsync(rc => rc.ReportCardId == id);

            if (card == null) return NotFound();
            return PartialView("_ReportCardView", card);
        }
    }

    public class GenerateReportRequest
    {
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
    }

    public class BulkPublishRequest
    {
        public int SessionId { get; set; }
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public bool IsPublish { get; set; }
    }
}
