using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class FeeStructureController : Controller
    {
        private readonly LibmanagementContext _context;

        public FeeStructureController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: FeeStructure/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: FeeStructure/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblFeeStructures
                .Where(f => f.IsActive == true)
                .Include(f => f.Session)
                .Include(f => f.Class)
                .Include(f => f.FeeType)
                .Select(f => new
                {
                    f.FeeStructureId,
                    SessionName = f.Session != null ? f.Session.SessionName : "-",
                    ClassName = f.Class != null ? f.Class.ClassName : "-",
                    FeeName = f.FeeType != null ? f.FeeType.FeeName : "-",
                    IsRecurring = f.FeeType != null && f.FeeType.IsRecurring == true ? "Monthly" : "One Time",
                    Amount = f.Amount != null ? "₹" + f.Amount.Value.ToString("0.00") : "₹0.00",
                    Status = f.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = f.CreatedDate.HasValue ? f.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: FeeStructure/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();

            if (id == 0)
                return PartialView("_FeeStructureModal", new TblFeeStructure { IsActive = true });

            var feeStructure = await _context.TblFeeStructures.FindAsync(id);
            if (feeStructure == null) return NotFound();

            return PartialView("_FeeStructureModal", feeStructure);
        }

        // POST: FeeStructure/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblFeeStructure model)
        {
            try
            {
                // Duplicate check — same session + class + feetype
                bool isDuplicate = await _context.TblFeeStructures
                    .AnyAsync(f => f.SessionId == model.SessionId
                               && f.ClassId == model.ClassId
                               && f.FeeTypeId == model.FeeTypeId
                               && f.IsActive == true
                               && f.FeeStructureId != id);

                if (isDuplicate)
                    return Json(new { success = false, message = "Fee structure already exists for this Session + Class + Fee Type combination!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblFeeStructures.Add(model);
                }
                else
                {
                    var existing = await _context.TblFeeStructures.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Fee Structure not found!" });

                    existing.SessionId = model.SessionId;
                    existing.ClassId = model.ClassId;
                    existing.FeeTypeId = model.FeeTypeId;
                    existing.Amount = model.Amount;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1; // Replace with logged-in user ID
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Fee Structure saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving fee structure! " + ex.Message });
            }
        }

        // POST: FeeStructure/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var feeStructure = await _context.TblFeeStructures.FindAsync(id);
            if (feeStructure == null)
                return Json(new { success = false, message = "Fee Structure not found!" });

            feeStructure.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Fee Structure deleted successfully!" });
        }

        // GET: FeeStructure/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var feeStructure = await _context.TblFeeStructures
                .Include(f => f.Session)
                .Include(f => f.Class)
                .Include(f => f.FeeType)
                .FirstOrDefaultAsync(f => f.FeeStructureId == id);

            if (feeStructure == null) return NotFound();

            return PartialView("_FeeStructureViewModal", feeStructure);
        }

        // GET: FeeStructure/GetDropdownData - Bulk modal ke liye
        [HttpGet]
        public async Task<IActionResult> GetDropdownData()
        {
            var sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            var classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();

            return Json(new { sessions, classes });
        }

        // GET: FeeStructure/GetBulkFeeData - Session+Class ke liye sabhi fee types + existing mapping
        [HttpGet]
        public async Task<IActionResult> GetBulkFeeData(int sessionId, int classId)
        {
            // Sirf Regular fee types — Optional FeeCategory wali FeeStructure mein nahi aayengi
            var allFeeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true && f.FeeCategory != "Optional")
                .OrderBy(f => f.FeeName)
                .ToListAsync();

            // Already mapped structures for this session+class
            var existing = await _context.TblFeeStructures
                .Where(f => f.SessionId == sessionId && f.ClassId == classId && f.IsActive == true)
                .ToListAsync();

            var result = allFeeTypes.Select(ft => {
                var map = existing.FirstOrDefault(e => e.FeeTypeId == ft.FeeTypeId);
                return new {
                    feeTypeId       = ft.FeeTypeId,
                    feeName         = ft.FeeName ?? "-",
                    isRecurring     = ft.IsRecurring == true,
                    isExisting      = map != null,
                    feeStructureId  = map?.FeeStructureId ?? 0,
                    existingAmount  = map?.Amount
                };
            }).ToList();

            return Json(result);
        }

        // POST: FeeStructure/SaveBulk - Bulk upsert
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveBulk([FromBody] BulkFeeStructureDto dto)
        {
            try
            {
                if (dto == null || dto.Items == null || !dto.Items.Any())
                    return Json(new { success = false, message = "Koi data nahi mila!" });

                int savedCount = 0;
                int updatedCount = 0;

                foreach (var item in dto.Items)
                {
                    if (item.FeeStructureId > 0)
                    {
                        // Update existing
                        var existing = await _context.TblFeeStructures.FindAsync(item.FeeStructureId);
                        if (existing != null)
                        {
                            existing.Amount      = item.Amount;
                            existing.IsActive    = true;
                            existing.UpdatedDate = DateTime.Now;
                            existing.UpdatedBy   = 1;
                            updatedCount++;
                        }
                    }
                    else
                    {
                        // Check duplicate before insert
                        bool exists = await _context.TblFeeStructures.AnyAsync(f =>
                            f.SessionId == dto.SessionId && f.ClassId == dto.ClassId &&
                            f.FeeTypeId == item.FeeTypeId && f.IsActive == true);

                        if (!exists)
                        {
                            _context.TblFeeStructures.Add(new TblFeeStructure
                            {
                                SessionId   = dto.SessionId,
                                ClassId     = dto.ClassId,
                                FeeTypeId   = item.FeeTypeId,
                                Amount      = item.Amount,
                                IsActive    = true,
                                CreatedDate = DateTime.Now,
                                CreatedBy   = 1
                            });
                            savedCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"{savedCount} new + {updatedCount} updated fee structures saved!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetFeeTypeInfo(int feeTypeId)
        {
            var feeType = await _context.TblFeeTypes
                .Where(f => f.FeeTypeId == feeTypeId)
                .Select(f => new { f.FeeName, IsRecurring = f.IsRecurring == true ? "Monthly" : "One Time" })
                .FirstOrDefaultAsync();

            if (feeType == null) return Json(new { success = false });
            return Json(new { success = true, feeType });
        }

        // ── Private helper ────────────────────────────────────────────────────
        private async Task LoadDropdowns()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            ViewBag.Classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();

            ViewBag.FeeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true && f.FeeCategory != "Optional")
                .OrderBy(f => f.FeeName)
                .Select(f => new { f.FeeTypeId, f.FeeName, IsRecurring = f.IsRecurring == true ? "Monthly" : "One Time" })
                .ToListAsync();
        }
    }

    // ── DTOs ──────────────────────────────────────────────────────────────────
    public class BulkFeeStructureDto
    {
        public int SessionId { get; set; }
        public int ClassId   { get; set; }
        public List<BulkFeeItem> Items { get; set; } = new();
    }

    public class BulkFeeItem
    {
        public int     FeeTypeId      { get; set; }
        public decimal Amount         { get; set; }
        public int     FeeStructureId { get; set; }  // 0 = new, >0 = update
    }
}
