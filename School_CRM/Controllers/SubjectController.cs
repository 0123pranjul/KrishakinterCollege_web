using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class SubjectController : Controller
    {
        private readonly LibmanagementContext _context;
        public SubjectController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblSubjects
                .Where(s => s.IsActive == true)
                .Select(s => new
                {
                    s.SubjectId,
                    s.SubjectName,
                    Status = s.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0) return PartialView("_SubjectModal", new TblSubject { IsActive = true });
            var item = await _context.TblSubjects.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_SubjectModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblSubject model)
        {
            try
            {
                bool isDuplicate = await _context.TblSubjects
                    .AnyAsync(s => s.SubjectName == model.SubjectName && s.IsActive == true && s.SubjectId != id);
                if (isDuplicate)
                    return Json(new { success = false, message = "Subject name already exists!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblSubjects.Add(model);
                }
                else
                {
                    var existing = await _context.TblSubjects.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.SubjectName = model.SubjectName;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Subject added successfully!" : "Subject updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblSubjects.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Subject deleted successfully!" });
        }
    }
}
