using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudyMaterialController : Controller
    {
        private readonly LibmanagementContext _context;
        public StudyMaterialController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblStudyMaterials
                .Where(m => m.IsActive == true)
                .Include(m => m.Teacher)
                .Include(m => m.Class)
                .Include(m => m.Section)
                .Include(m => m.Subject)
                .Select(m => new
                {
                    m.MaterialId,
                    m.Title,
                    TeacherName = m.Teacher.TeacherName,
                    ClassName = m.Class.ClassName,
                    SectionName = m.Section.SectionName,
                    SubjectName = m.Subject.SubjectName,
                    HasFile = !string.IsNullOrEmpty(m.FilePath),
                    Status = m.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = m.CreatedDate.HasValue ? m.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();
            if (id == 0) return PartialView("_StudyMaterialModal", new TblStudyMaterial { IsActive = true });
            var item = await _context.TblStudyMaterials.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_StudyMaterialModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudyMaterial model, IFormFile? file)
        {
            try
            {
                if (file != null && file.Length > 0)
                {
                    var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "study-materials");
                    Directory.CreateDirectory(uploadsDir);
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsDir, fileName);
                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);
                    model.FilePath = $"/uploads/study-materials/{fileName}";
                }

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblStudyMaterials.Add(model);
                }
                else
                {
                    var existing = await _context.TblStudyMaterials.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.Title = model.Title;
                    existing.Content = model.Content;
                    existing.TeacherId = model.TeacherId;
                    existing.ClassId = model.ClassId;
                    existing.SectionId = model.SectionId;
                    existing.SubjectId = model.SubjectId;
                    if (!string.IsNullOrEmpty(model.FilePath)) existing.FilePath = model.FilePath;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Material added successfully!" : "Material updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblStudyMaterials.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Material deleted successfully!" });
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
