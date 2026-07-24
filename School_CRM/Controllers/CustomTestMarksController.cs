using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class CustomTestMarksController : Controller
    {
        private readonly LibmanagementContext _context;
        public CustomTestMarksController(LibmanagementContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Tests = await _context.TblCustomTests.Where(t => t.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForMarks(int testId, int classId, int sectionId)
        {
            var test = await _context.TblCustomTests.FindAsync(testId);
            if (test == null) return Json(new { data = new List<object>() });

            var students = await _context.TblStudentSessions
                .Where(ss => ss.ClassId == classId && ss.SectionId == sectionId && ss.IsActive == true && ss.StudentId != null)
                .Include(ss => ss.Student)
                .ToListAsync();

            var existingMarks = await _context.TblCustomTestMarks
                .Where(m => m.TestId == testId)
                .ToListAsync();

            var data = students.Select(ss => {
                var mark = existingMarks.FirstOrDefault(m => m.StudentId == ss.StudentId!.Value);
                return new
                {
                    StudentId = ss.StudentId!.Value,
                    StudentName = ss.Student?.StudentName ?? "-",
                    MarkId = mark?.Id ?? 0,
                    MarksObtained = mark?.MarksObtained,
                    IsAbsent = mark?.IsAbsent ?? false,
                    MaxMarks = test.MaxMarks
                };
            }).ToList();

            return Json(new { data, maxMarks = test.MaxMarks });
        }

        [HttpPost]
        public async Task<IActionResult> SaveMarks([FromBody] List<CustomTestMarkEntry> entries)
        {
            try
            {
                foreach (var entry in entries)
                {
                    if (entry.MarkId == 0)
                    {
                        _context.TblCustomTestMarks.Add(new TblCustomTestMark
                        {
                            TestId = entry.TestId,
                            StudentId = entry.StudentId,
                            MarksObtained = entry.IsAbsent ? null : entry.MarksObtained,
                            IsAbsent = entry.IsAbsent,
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        var existing = await _context.TblCustomTestMarks.FindAsync(entry.MarkId);
                        if (existing != null)
                        {
                            existing.MarksObtained = entry.IsAbsent ? null : entry.MarksObtained;
                            existing.IsAbsent = entry.IsAbsent;
                            existing.UpdatedDate = DateTime.Now;
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Test Marks saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class CustomTestMarkEntry
    {
        public int MarkId { get; set; }
        public int TestId { get; set; }
        public int StudentId { get; set; }
        public decimal? MarksObtained { get; set; }
        public bool IsAbsent { get; set; }
    }
}
