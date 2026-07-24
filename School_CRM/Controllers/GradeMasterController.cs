using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class GradeMasterController : Controller
    {
        private readonly LibmanagementContext _context;
        public GradeMasterController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblGradeMasters
                .Where(g => g.IsActive == true)
                .OrderByDescending(g => g.MinPercent)
                .Select(g => new
                {
                    g.GradeId,
                    g.GradeName,
                    MinPercent = g.MinPercent.ToString("0.00") + "%",
                    MaxPercent = g.MaxPercent.ToString("0.00") + "%",
                    GradePoint = g.GradePoint.ToString("0.0"),
                    g.Remark,
                    Status = g.IsActive == true ? "Active" : "Inactive"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0) return PartialView("_GradeMasterModal", new TblGradeMaster { IsActive = true });
            var item = await _context.TblGradeMasters.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_GradeMasterModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblGradeMaster model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblGradeMasters.Add(model);
                }
                else
                {
                    var existing = await _context.TblGradeMasters.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.GradeName = model.GradeName;
                    existing.MinPercent = model.MinPercent;
                    existing.MaxPercent = model.MaxPercent;
                    existing.GradePoint = model.GradePoint;
                    existing.Remark = model.Remark;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Grade added successfully!" : "Grade updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblGradeMasters.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Grade deleted successfully!" });
        }
    }
}
