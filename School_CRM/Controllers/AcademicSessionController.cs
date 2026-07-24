using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class AcademicSessionController : Controller
    {
        private readonly LibmanagementContext _context;

        public AcademicSessionController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: AcademicSession/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: AcademicSession/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .Select(s => new
                {
                    s.SessionId,
                    s.SessionName,
                    StartDate = s.StartDate.HasValue ? s.StartDate.Value.ToString("dd-MM-yyyy") : "-",
                    EndDate = s.EndDate.HasValue ? s.EndDate.Value.ToString("dd-MM-yyyy") : "-",
                    Status = s.IsActive == true ? "Active" : "Inactive"
                })
                .ToListAsync();

            return Json(new { data = sessions });
        }

        // GET: AcademicSession/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("_AcademicSessionModal", new TblAcademicSession());

            var session = await _context.TblAcademicSessions.FindAsync(id);
            if (session == null) return NotFound();

            return PartialView("_AcademicSessionModal", session);
        }

        // POST: AcademicSession/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblAcademicSession session)
        {
            try
            {
                if (id == 0)
                {
                    session.IsActive = true;
                    session.CreatedDate = DateTime.Now;
                    session.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblAcademicSessions.Add(session);
                }
                else
                {
                    var existing = await _context.TblAcademicSessions.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Session not found!" });

                    existing.SessionName = session.SessionName;
                    existing.StartDate = session.StartDate;
                    existing.EndDate = session.EndDate;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1; // Replace with logged-in user ID
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Academic Session saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving session! " + ex.Message });
            }
        }

        // POST: AcademicSession/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var session = await _context.TblAcademicSessions.FindAsync(id);
            if (session == null)
                return Json(new { success = false, message = "Session not found!" });

            session.IsActive = false; // Soft Delete
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Academic Session deleted successfully!" });
        }

        // GET: AcademicSession/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var session = await _context.TblAcademicSessions
                .Include(s => s.TblFeeCollections)
                .Include(s => s.TblFeeStructures)
                .Include(s => s.TblStudentSessions)
                .FirstOrDefaultAsync(s => s.SessionId == id);

            if (session == null) return NotFound();

            return PartialView("_AcademicSessionViewModal", session);
        }
    }
}
