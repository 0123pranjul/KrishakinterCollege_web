using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudentSessionController : Controller
    {
        private readonly LibmanagementContext _context;

        public StudentSessionController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: StudentSession/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: StudentSession/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblStudentSessions
                .Where(s => s.IsActive == true)
                .Include(s => s.Student)
                .Include(s => s.Session)
                .Include(s => s.Class)
                .Include(s => s.Section)
                .Select(s => new
                {
                    s.Id,
                    StudentName = s.Student != null ? s.Student.StudentName : "-",
                    RollNo = s.Student != null ? s.Student.RollNo : "-",
                    SessionName = s.Session != null ? s.Session.SessionName : "-",
                    ClassName = s.Class != null ? s.Class.ClassName : "-",
                    SectionName = s.Section != null ? s.Section.SectionName : "-",
                    Status = s.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: StudentSession/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            // Load all dropdowns
            ViewBag.Students = await _context.TblStudents
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.StudentName)
                .Select(s => new { s.StudentId, s.StudentName, s.RollNo, DisplayName = s.StudentName + " | Roll: " + s.RollNo })
                .ToListAsync();

            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            ViewBag.Classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();

            ViewBag.Sections = await _context.TblSections
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName })
                .ToListAsync();

            if (id == 0)
                return PartialView("_StudentSessionModal", new TblStudentSession { IsActive = true });

            var mapping = await _context.TblStudentSessions.FindAsync(id);
            if (mapping == null) return NotFound();

            return PartialView("_StudentSessionModal", mapping);
        }

        // POST: StudentSession/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudentSession model)
        {
            try
            {
                // Duplicate check - same student in same session
                bool isDuplicate = await _context.TblStudentSessions
                    .AnyAsync(s => s.StudentId == model.StudentId
                               && s.SessionId == model.SessionId
                               && s.IsActive == true
                               && s.Id != id);

                if (isDuplicate)
                    return Json(new { success = false, message = "This student is already mapped to the selected session!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblStudentSessions.Add(model);
                }
                else
                {
                    var existing = await _context.TblStudentSessions.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Mapping not found!" });

                    existing.StudentId = model.StudentId;
                    existing.SessionId = model.SessionId;
                    existing.ClassId = model.ClassId;
                    existing.SectionId = model.SectionId;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1; // Replace with logged-in user ID
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student session mapping saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving mapping! " + ex.Message });
            }
        }

        // POST: StudentSession/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var mapping = await _context.TblStudentSessions.FindAsync(id);
            if (mapping == null)
                return Json(new { success = false, message = "Mapping not found!" });

            mapping.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Mapping deleted successfully!" });
        }

        // GET: StudentSession/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var mapping = await _context.TblStudentSessions
                .Include(s => s.Student)
                .Include(s => s.Session)
                .Include(s => s.Class)
                .Include(s => s.Section)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (mapping == null) return NotFound();

            return PartialView("_StudentSessionViewModal", mapping);
        }

        // GET: StudentSession/GetStudentInfo/5 - AJAX student info
        [HttpGet]
        public async Task<IActionResult> GetStudentInfo(int studentId)
        {
            var student = await _context.TblStudents
                .Where(s => s.StudentId == studentId)
                .Select(s => new { s.StudentName, s.RollNo })
                .FirstOrDefaultAsync();

            if (student == null)
                return Json(new { success = false });

            return Json(new { success = true, student });
        }
    }
}
