using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class AssignmentController : Controller
    {
        private readonly LibmanagementContext _context;
        public AssignmentController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblAssignments
                .Where(a => a.IsActive == true)
                .Include(a => a.Teacher)
                .Include(a => a.Class)
                .Include(a => a.Section)
                .Include(a => a.Subject)
                .Include(a => a.Session)
                .Select(a => new
                {
                    a.AssignmentId,
                    a.Title,
                    TeacherName = a.Teacher.TeacherName,
                    ClassName = a.Class.ClassName,
                    SectionName = a.Section.SectionName,
                    SubjectName = a.Subject.SubjectName,
                    SessionName = a.Session.SessionName,
                    DueDate = a.DueDate.ToString("dd-MM-yyyy"),
                    Status = a.IsActive == true ? "Active" : "Inactive"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();
            if (id == 0) return PartialView("_AssignmentModal", new TblAssignment { IsActive = true, DueDate = DateOnly.FromDateTime(DateTime.Today.AddDays(7)) });
            var item = await _context.TblAssignments.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_AssignmentModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblAssignment model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblAssignments.Add(model);
                }
                else
                {
                    var existing = await _context.TblAssignments.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.Title = model.Title;
                    existing.Description = model.Description;
                    existing.TeacherId = model.TeacherId;
                    existing.ClassId = model.ClassId;
                    existing.SectionId = model.SectionId;
                    existing.SubjectId = model.SubjectId;
                    existing.SessionId = model.SessionId;
                    existing.DueDate = model.DueDate;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Assignment added successfully!" : "Assignment updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblAssignments.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Assignment deleted successfully!" });
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Teachers = await _context.TblTeachers.Where(t => t.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
        }
    }
}
