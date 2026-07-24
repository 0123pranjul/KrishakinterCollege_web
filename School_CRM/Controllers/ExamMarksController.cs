using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ExamMarksController : Controller
    {
        private readonly LibmanagementContext _context;
        public ExamMarksController(LibmanagementContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Exams = await _context.TblExams.Where(e => e.IsActive == true).Include(e => e.Session).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForMarks(int examId, int classId, int sectionId, int subjectId)
        {
            var students = await _context.TblStudentSessions
                .Where(ss => ss.ClassId == classId && ss.SectionId == sectionId && ss.IsActive == true && ss.StudentId != null)
                .Include(ss => ss.Student)
                .ToListAsync();

            var existingMarks = await _context.TblExamMarks
                .Where(m => m.ExamId == examId && m.SubjectId == subjectId && m.IsActive == true)
                .ToListAsync();

            var examSubject = await _context.TblExamSubjects
                .FirstOrDefaultAsync(es => es.ExamId == examId && es.ClassId == classId && es.SubjectId == subjectId && es.IsActive == true);
            
            decimal passMarks = examSubject?.PassMarks ?? 33;
            decimal maxMarks = examSubject?.MaxMarks ?? 100;

            var data = students.Select(ss => {
                var mark = existingMarks.FirstOrDefault(m => m.StudentId == ss.StudentId!.Value);
                return new
                {
                    StudentId = ss.StudentId!.Value,
                    StudentName = ss.Student?.StudentName ?? "-",
                    MarkId = mark?.Id ?? 0,
                    MarksObtained = mark?.MarksObtained,
                    IsAbsent = mark?.IsAbsent ?? false
                };
            }).ToList();

            return Json(new { data, passMarks, maxMarks });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMarks([FromBody] List<ExamMarkEntry> entries)
        {
            try
            {
                foreach (var entry in entries)
                {
                    if (entry.MarkId == 0)
                    {
                        _context.TblExamMarks.Add(new TblExamMark
                        {
                            ExamId = entry.ExamId,
                            StudentId = entry.StudentId,
                            SubjectId = entry.SubjectId,
                            MarksObtained = entry.IsAbsent ? null : entry.MarksObtained,
                            IsAbsent = entry.IsAbsent,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        var existing = await _context.TblExamMarks.FindAsync(entry.MarkId);
                        if (existing != null)
                        {
                            existing.MarksObtained = entry.IsAbsent ? null : entry.MarksObtained;
                            existing.IsAbsent = entry.IsAbsent;
                            existing.UpdatedDate = DateTime.Now;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Marks saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class ExamMarkEntry
    {
        public int MarkId { get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public int SubjectId { get; set; }
        public decimal? MarksObtained { get; set; }
        public bool IsAbsent { get; set; }
    }
}
