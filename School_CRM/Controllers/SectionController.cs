using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class SectionController : Controller
    {
        private readonly LibmanagementContext _context;

        public SectionController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: Section/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Section/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sections = await _context.TblSections
                .Where(s => s.IsActive == true)
                .Select(s => new
                {
                    s.SectionId,
                    s.SectionName,
                    Status = s.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data = sections });
        }

        // GET: Section/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("_SectionModal", new TblSection { IsActive = true });

            var section = await _context.TblSections.FindAsync(id);
            if (section == null) return NotFound();

            return PartialView("_SectionModal", section);
        }

        // POST: Section/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblSection section)
        {
            try
            {
                if (id == 0)
                {
                    section.CreatedDate = DateTime.Now;
                    section.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblSections.Add(section);
                }
                else
                {
                    var existing = await _context.TblSections.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Section not found!" });

                    existing.SectionName = section.SectionName;
                    existing.IsActive = section.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1; // Replace with logged-in user ID
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Section saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving section! " + ex.Message });
            }
        }

        // POST: Section/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var section = await _context.TblSections.FindAsync(id);
            if (section == null)
                return Json(new { success = false, message = "Section not found!" });

            section.IsActive = false; // Soft Delete
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Section deleted successfully!" });
        }

        // GET: Section/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var section = await _context.TblSections
                .Include(s => s.TblStudentSessions)
                .FirstOrDefaultAsync(s => s.SectionId == id);

            if (section == null) return NotFound();

            return PartialView("_SectionViewModal", section);
        }
    }
}
