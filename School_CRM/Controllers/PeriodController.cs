using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class PeriodController : Controller
    {
        private readonly LibmanagementContext _context;
        public PeriodController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblPeriods
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.SequenceNo)
                .Select(p => new
                {
                    p.PeriodId,
                    p.PeriodName,
                    StartTime = p.StartTime.ToString("hh\\:mm tt"),
                    EndTime = p.EndTime.ToString("hh\\:mm tt"),
                    p.SequenceNo,
                    IsBrake = p.IsBrake == true ? "Yes" : "No",
                    Status = p.IsActive == true ? "Active" : "Inactive"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0) return PartialView("_PeriodModal", new TblPeriod { IsActive = true, IsBrake = false });
            var item = await _context.TblPeriods.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_PeriodModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblPeriod model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblPeriods.Add(model);
                }
                else
                {
                    var existing = await _context.TblPeriods.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.PeriodName = model.PeriodName;
                    existing.StartTime = model.StartTime;
                    existing.EndTime = model.EndTime;
                    existing.SequenceNo = model.SequenceNo;
                    existing.IsBrake = model.IsBrake;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Period added successfully!" : "Period updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblPeriods.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Period deleted successfully!" });
        }
    }
}
