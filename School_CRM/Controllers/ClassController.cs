using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ClassController : Controller
    {
        private readonly LibmanagementContext _context;

        public ClassController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: Class/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: Class/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .Select(c => new
                {
                    c.ClassId,
                    c.ClassName,
                    Status = c.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = c.CreatedDate.HasValue ? c.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data = classes });
        }

        // GET: Class/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("_ClassModal", new TblClass());

            var tblClass = await _context.TblClasses.FindAsync(id);
            if (tblClass == null) return NotFound();

            return PartialView("_ClassModal", tblClass);
        }

        // POST: Class/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblClass tblClass)
        {
            try
            {
                if (id == 0)
                {
                    tblClass.IsActive = true;
                    tblClass.CreatedDate = DateTime.Now;
                    tblClass.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblClasses.Add(tblClass);
                }
                else
                {
                    var existing = await _context.TblClasses.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Class not found!" });

                    existing.ClassName = tblClass.ClassName;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1; // Replace with logged-in user ID
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Class saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving class! " + ex.Message });
            }
        }

        // POST: Class/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var tblClass = await _context.TblClasses.FindAsync(id);
            if (tblClass == null)
                return Json(new { success = false, message = "Class not found!" });

            tblClass.IsActive = false; // Soft Delete
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Class deleted successfully!" });
        }

        // GET: Class/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var tblClass = await _context.TblClasses
                .Include(c => c.TblFeeStructures)
                .Include(c => c.TblStudentSessions)
                .FirstOrDefaultAsync(c => c.ClassId == id);

            if (tblClass == null) return NotFound();

            return PartialView("_ClassViewModal", tblClass);
        }
    }
}
