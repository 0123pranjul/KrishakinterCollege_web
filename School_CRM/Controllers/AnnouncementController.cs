using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly LibmanagementContext _context;
        public AnnouncementController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblAnnouncements
                .Where(a => a.IsActive == true)
                .Select(a => new
                {
                    a.AnnouncementId,
                    a.Title,
                    Scope = a.IsGlobal == true ? "All Classes" : $"Class {a.ClassId} / Section {a.SectionId}",
                    IsGlobal = a.IsGlobal == true ? "Global" : "Targeted",
                    Status = a.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = a.CreatedDate.HasValue ? a.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            if (id == 0) return PartialView("_AnnouncementModal", new TblAnnouncement { IsActive = true, IsGlobal = false });
            var item = await _context.TblAnnouncements.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_AnnouncementModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblAnnouncement model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblAnnouncements.Add(model);
                }
                else
                {
                    var existing = await _context.TblAnnouncements.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.Title = model.Title;
                    existing.Message = model.Message;
                    existing.ClassId = model.IsGlobal == true ? null : model.ClassId;
                    existing.SectionId = model.IsGlobal == true ? null : model.SectionId;
                    existing.IsGlobal = model.IsGlobal;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Announcement added successfully!" : "Announcement updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblAnnouncements.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Announcement deleted successfully!" });
        }
    }
}
