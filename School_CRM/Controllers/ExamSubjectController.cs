using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Globalization;

namespace School_CRM.Controllers
{
    public class ExamSubjectController : Controller
    {
        private readonly LibmanagementContext _context;
        public ExamSubjectController(LibmanagementContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Exams = await _context.TblExams.Where(e => e.IsActive == true).Include(e => e.Session).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectsForBulkMap(int examId, int classId)
        {
            var classSubjects = await _context.TblClassSubjects
                .Where(cs => cs.ClassId == classId && cs.IsActive == true)
                .Include(cs => cs.Subject)
                .ToListAsync();

            var existingMappings = await _context.TblExamSubjects
                .Where(es => es.ExamId == examId && es.ClassId == classId && es.IsActive == true)
                .ToListAsync();

            var data = classSubjects.Select(cs => {
                var existing = existingMappings.FirstOrDefault(e => e.SubjectId == cs.SubjectId);
                return new
                {
                    SubjectId = cs.SubjectId,
                    SubjectName = cs.Subject.SubjectName,
                    Id = existing?.Id ?? 0,
                    MaxMarks = existing?.MaxMarks ?? 100,
                    PassMarks = existing?.PassMarks ?? 33,
                    ExamDate = existing?.ExamDate?.ToString("yyyy-MM-dd"),
                    ExamTime = existing?.ExamTime ?? "",
                    RoomNo = existing?.RoomNo ?? ""
                };
            }).OrderBy(x => x.SubjectName).ToList();

            return Json(new { data });
        }

        [HttpPost]
        public async Task<IActionResult> SaveBulkMapping([FromBody] List<BulkExamSubjectDto> data)
        {
            try
            {
                if (data == null || !data.Any())
                    return Json(new { success = false, message = "No data provided!" });

                int examId = data.First().ExamId;
                int classId = data.First().ClassId;

                foreach (var item in data)
                {
                    DateOnly? parsedDate = null;
                    if (!string.IsNullOrEmpty(item.ExamDate) && DateOnly.TryParse(item.ExamDate, out DateOnly d))
                        parsedDate = d;

                    if (item.Id == 0)
                    {
                        _context.TblExamSubjects.Add(new TblExamSubject
                        {
                            ExamId = item.ExamId,
                            ClassId = item.ClassId,
                            SubjectId = item.SubjectId,
                            MaxMarks = item.MaxMarks,
                            PassMarks = item.PassMarks,
                            ExamDate = parsedDate,
                            ExamTime = item.ExamTime,
                            RoomNo = item.RoomNo,
                            IsActive = true,
                            CreatedDate = DateTime.Now
                        });
                    }
                    else
                    {
                        var existing = await _context.TblExamSubjects.FindAsync(item.Id);
                        if (existing != null)
                        {
                            existing.MaxMarks = item.MaxMarks;
                            existing.PassMarks = item.PassMarks;
                            existing.ExamDate = parsedDate;
                            existing.ExamTime = item.ExamTime;
                            existing.RoomNo = item.RoomNo;
                            existing.UpdatedDate = DateTime.Now;
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Exam Timetable & Marks saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class BulkExamSubjectDto
    {
        public int Id { get; set; }
        public int ExamId { get; set; }
        public int ClassId { get; set; }
        public int SubjectId { get; set; }
        public decimal MaxMarks { get; set; }
        public decimal PassMarks { get; set; }
        public string? ExamDate { get; set; }
        public string? ExamTime { get; set; }
        public string? RoomNo { get; set; }
    }
}
