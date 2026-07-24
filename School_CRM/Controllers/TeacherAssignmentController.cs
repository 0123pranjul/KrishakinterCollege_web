using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class TeacherAssignmentController : Controller
    {
        private readonly LibmanagementContext _context;
        public TeacherAssignmentController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblTeacherAssignments
                .Where(ta => ta.IsActive == true)
                .Include(ta => ta.Teacher)
                .Include(ta => ta.Class)
                .Include(ta => ta.Section)
                .Include(ta => ta.Subject)
                .Include(ta => ta.Session)
                .Select(ta => new
                {
                    ta.Id,
                    TeacherName = ta.Teacher.TeacherName,
                    ClassName = ta.Class.ClassName,
                    SectionName = ta.Section.SectionName,
                    SubjectName = ta.Subject.SubjectName,
                    SessionName = ta.Session.SessionName,
                    Status = ta.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = ta.CreatedDate.HasValue ? ta.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();
            if (id == 0) return PartialView("_TeacherAssignmentModal", new TblTeacherAssignment { IsActive = true });
            var item = await _context.TblTeacherAssignments.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_TeacherAssignmentModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblTeacherAssignment model)
        {
            try
            {
                bool isDuplicate = await _context.TblTeacherAssignments
                    .AnyAsync(ta => ta.ClassId == model.ClassId && ta.SectionId == model.SectionId
                        && ta.SubjectId == model.SubjectId && ta.SessionId == model.SessionId
                        && ta.IsActive == true && ta.Id != id);
                if (isDuplicate)
                    return Json(new { success = false, message = "A teacher is already assigned for this Class + Section + Subject + Session!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTeacherAssignments.Add(model);
                }
                else
                {
                    var existing = await _context.TblTeacherAssignments.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.TeacherId = model.TeacherId;
                    existing.ClassId = model.ClassId;
                    existing.SectionId = model.SectionId;
                    existing.SubjectId = model.SubjectId;
                    existing.SessionId = model.SessionId;
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
            var item = await _context.TblTeacherAssignments.FindAsync(id);
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
