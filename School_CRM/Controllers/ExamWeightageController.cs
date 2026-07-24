using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ExamWeightageController : Controller
    {
        private readonly LibmanagementContext _context;
        public ExamWeightageController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblExamWeightages
                .Where(ew => ew.IsActive == true)
                .Include(ew => ew.Session)
                .Include(ew => ew.Exam)
                .Select(ew => new
                {
                    ew.Id,
                    SessionName = ew.Session.SessionName,
                    ExamName = ew.Exam.ExamName,
                    WeightPct = ew.WeightPct.ToString("0.00") + "%",
                    Status = ew.IsActive == true ? "Active" : "Inactive"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Exams = await _context.TblExams.Where(e => e.IsActive == true).ToListAsync();
            if (id == 0) return PartialView("_ExamWeightageModal", new TblExamWeightage { IsActive = true });
            var item = await _context.TblExamWeightages.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_ExamWeightageModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblExamWeightage model)
        {
            try
            {
                bool isDuplicate = await _context.TblExamWeightages
                    .AnyAsync(ew => ew.SessionId == model.SessionId && ew.ExamId == model.ExamId && ew.IsActive == true && ew.Id != id);
                if (isDuplicate)
                    return Json(new { success = false, message = "Weightage already set for this Session + Exam!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblExamWeightages.Add(model);
                }
                else
                {
                    var existing = await _context.TblExamWeightages.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.SessionId = model.SessionId;
                    existing.ExamId = model.ExamId;
                    existing.WeightPct = model.WeightPct;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Weightage added successfully!" : "Weightage updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblExamWeightages.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Weightage deleted successfully!" });
        }
    }
}
