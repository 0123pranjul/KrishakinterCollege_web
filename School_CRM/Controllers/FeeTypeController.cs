using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class FeeTypeController : Controller
    {
        private readonly LibmanagementContext _context;

        public FeeTypeController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: FeeType/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: FeeType/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var feeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true)
                .OrderBy(f => f.FeeTypeId)
                .Select(f => new
                {
                    f.FeeTypeId,
                    f.FeeName,
                    f.FeeCategory,
                    FeeTypeBadge = f.FeeCategory == "Optional"
                        ? "Optional"
                        : f.IsRecurring == true ? "Monthly" : "One Time",
                    Status = f.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = f.CreatedDate.HasValue ? f.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data = feeTypes });
        }

        // GET: FeeType/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("_FeeTypeModal", new TblFeeType { IsActive = true, IsRecurring = false });

            var feeType = await _context.TblFeeTypes.FindAsync(id);
            if (feeType == null) return NotFound();

            return PartialView("_FeeTypeModal", feeType);
        }

        // POST: FeeType/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblFeeType feeType)
        {
            try
            {
                if (id == 0)
                {
                    feeType.CreatedDate = DateTime.Now;
                    feeType.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblFeeTypes.Add(feeType);
                }
                else
                {
                    var existing = await _context.TblFeeTypes.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Fee Type not found!" });

                    existing.FeeName = feeType.FeeName;
                    existing.IsRecurring = feeType.IsRecurring;
                    existing.FeeCategory = feeType.FeeCategory;
                    existing.IsActive = feeType.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1; // Replace with logged-in user ID
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Fee Type saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving fee type! " + ex.Message });
            }
        }

        // POST: FeeType/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var feeType = await _context.TblFeeTypes.FindAsync(id);
            if (feeType == null)
                return Json(new { success = false, message = "Fee Type not found!" });

            feeType.IsActive = false; // Soft Delete
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Fee Type deleted successfully!" });
        }

        // GET: FeeType/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var feeType = await _context.TblFeeTypes
                .Include(f => f.TblFeeStructures)
                .Include(f => f.TblFeeCollectionDetails)
                .Include(f => f.TblStudentExtraCharges)
                .Include(f => f.TblStudentFeeOverrides)
                .FirstOrDefaultAsync(f => f.FeeTypeId == id);

            if (feeType == null) return NotFound();

            return PartialView("_FeeTypeViewModal", feeType);
        }
    }
}
