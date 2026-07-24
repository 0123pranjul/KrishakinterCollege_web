using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class CustomTestController : Controller
    {
        private readonly LibmanagementContext _context;
        public CustomTestController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblCustomTests
                .Where(t => t.IsActive == true)
                .Include(t => t.Teacher)
                .Include(t => t.Class)
                .Include(t => t.Section)
                .Include(t => t.Subject)
                .Select(t => new
                {
                    t.TestId,
                    t.TestName,
                    TeacherName = t.Teacher.TeacherName,
                    ClassName = t.Class.ClassName,
                    SectionName = t.Section.SectionName,
                    SubjectName = t.Subject.SubjectName,
                    MaxMarks = t.MaxMarks.ToString("0.00"),
                    TestDate = t.TestDate.ToString("dd-MM-yyyy"),
                    Status = t.IsActive == true ? "Active" : "Inactive"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();
            if (id == 0) return PartialView("_CustomTestModal", new TblCustomTest { IsActive = true, TestDate = DateOnly.FromDateTime(DateTime.Today) });
            var item = await _context.TblCustomTests.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_CustomTestModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblCustomTest model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblCustomTests.Add(model);
                }
                else
                {
                    var existing = await _context.TblCustomTests.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.TestName = model.TestName;
                    existing.TeacherId = model.TeacherId;
                    existing.ClassId = model.ClassId;
                    existing.SectionId = model.SectionId;
                    existing.SubjectId = model.SubjectId;
                    existing.MaxMarks = model.MaxMarks;
                    existing.TestDate = model.TestDate;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Test added successfully!" : "Test updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblCustomTests.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Test deleted successfully!" });
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Teachers = await _context.TblTeachers.Where(t => t.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
        }
    }
}
