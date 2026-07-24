using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ExamController : Controller
    {
        private readonly LibmanagementContext _context;
        public ExamController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblExams
                .Where(e => e.IsActive == true)
                .Include(e => e.Session)
                .Select(e => new
                {
                    e.ExamId,
                    e.ExamName,
                    SessionName = e.Session.SessionName,
                    Status = e.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = e.CreatedDate.HasValue ? e.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            if (id == 0) return PartialView("_ExamModal", new TblExam { IsActive = true });
            var item = await _context.TblExams.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_ExamModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblExam model)
        {
            try
            {
                bool isDuplicate = await _context.TblExams
                    .AnyAsync(e => e.ExamName == model.ExamName && e.SessionId == model.SessionId && e.IsActive == true && e.ExamId != id);
                if (isDuplicate)
                    return Json(new { success = false, message = "Exam already exists for this session!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblExams.Add(model);
                }
                else
                {
                    var existing = await _context.TblExams.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.ExamName = model.ExamName;
                    existing.SessionId = model.SessionId;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Exam added successfully!" : "Exam updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblExams.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Exam deleted successfully!" });
        }

        [HttpGet]
        public async Task<IActionResult> AdmitCardIndex()
        {
            ViewBag.Exams = await _context.TblExams.Where(e => e.IsActive == true).Include(e => e.Session).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsForAdmitCard(int examId, int classId, int sectionId)
        {
            var exam = await _context.TblExams.FindAsync(examId);
            if (exam == null) return Json(new { data = new List<object>() });

            var query = _context.TblStudentSessions
                .Where(ss => ss.SessionId == exam.SessionId && ss.ClassId == classId && ss.IsActive == true && ss.StudentId != null)
                .Include(ss => ss.Student)
                .AsQueryable();

            if (sectionId > 0) query = query.Where(ss => ss.SectionId == sectionId);

            var students = await query.Select(ss => new
            {
                StudentId = ss.StudentId,
                StudentName = ss.Student.StudentName,
                RollNo = ss.Student.RollNo
            }).ToListAsync();

            return Json(new { data = students });
        }

        [HttpGet]
        public async Task<IActionResult> AdmitCard(int examId, int classId, int sectionId, int studentId = 0)
        {
            if (examId == 0 || classId == 0) return NotFound();

            var exam = await _context.TblExams.Include(e => e.Session).FirstOrDefaultAsync(e => e.ExamId == examId);
            if (exam == null) return NotFound();

            var query = _context.TblStudentSessions
                .Include(ss => ss.Student)
                .Include(ss => ss.Class)
                .Include(ss => ss.Section)
                .Where(ss => ss.SessionId == exam.SessionId && ss.ClassId == classId && ss.IsActive == true && ss.StudentId != null);
            
            if (sectionId > 0) query = query.Where(ss => ss.SectionId == sectionId);
            if (studentId > 0) query = query.Where(ss => ss.StudentId == studentId);

            var students = await query.ToListAsync();

            var examSubjects = await _context.TblExamSubjects
                .Include(es => es.Subject)
                .Where(es => es.ExamId == examId && es.ClassId == classId && es.IsActive == true)
                .OrderBy(es => es.ExamDate)
                .ToListAsync();

            ViewBag.Exam = exam;
            ViewBag.ExamSubjects = examSubjects;

            return View(students);
        }
    }
}
