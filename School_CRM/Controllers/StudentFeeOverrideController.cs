using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudentFeeOverrideController : Controller
    {
        private readonly LibmanagementContext _context;

        public StudentFeeOverrideController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: StudentFeeOverride/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: StudentFeeOverride/SearchStudents - Select2 AJAX search
        [HttpGet]
        public async Task<IActionResult> SearchStudents(string? term, int page = 1)
        {
            const int pageSize = 20;

            var query = _context.TblStudentSessions
                .Where(ss => ss.IsActive == true && ss.Student != null && ss.Student.IsActive == true)
                .Include(ss => ss.Student)
                .Include(ss => ss.Class)
                .Include(ss => ss.Section)
                .Include(ss => ss.Session)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                string t = term.Trim().ToLower();
                query = query.Where(ss =>
                    (ss.Student!.StudentName != null && ss.Student.StudentName.ToLower().Contains(t)) ||
                    (ss.Student!.RollNo      != null && ss.Student.RollNo.ToLower().Contains(t))      ||
                    (ss.Student!.AdmissionNo != null && ss.Student.AdmissionNo.ToLower().Contains(t)));
            }

            var total = await query
                .Select(ss => ss.StudentId)
                .Distinct()
                .CountAsync();

            var data = await query
                .OrderBy(ss => ss.Student!.StudentName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ss => new {
                    id          = ss.Student!.StudentId,
                    text        = ss.Student.StudentName + " | Roll: " + ss.Student.RollNo,
                    name        = ss.Student.StudentName ?? "-",
                    roll        = ss.Student.RollNo      ?? "-",
                    className   = ss.Class   != null ? ss.Class.ClassName     : "-",
                    sectionName = ss.Section != null ? ss.Section.SectionName : "-",
                    sessionName = ss.Session != null ? ss.Session.SessionName : "-"
                })
                .ToListAsync();

            return Json(new {
                results    = data,
                pagination = new { more = (page * pageSize) < total }
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblStudentFeeOverrides
                .Where(s => s.IsActive == true)
                .Include(s => s.Student)
                .Include(s => s.FeeType)
                .Select(s => new
                {
                    s.Id,
                    StudentName = s.Student != null ? s.Student.StudentName : "-",
                    RollNo = s.Student != null ? s.Student.RollNo : "-",
                    FeeName = s.FeeType != null ? s.FeeType.FeeName : "-",
                    IsRecurring = s.FeeType != null && s.FeeType.IsRecurring == true ? "Monthly" : "One Time",
                    Amount = s.Amount != null ? "₹" + s.Amount.Value.ToString("0.00") : "₹0.00",
                    Status = s.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: StudentFeeOverride/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();

            if (id == 0)
                return PartialView("_StudentFeeOverrideModal", new TblStudentFeeOverride { IsActive = true });

            var record = await _context.TblStudentFeeOverrides.FindAsync(id);
            if (record == null) return NotFound();

            // Edit mode: sirf selected student ka naam chahiye (Select2 ke liye)
            if (record.StudentId.HasValue)
            {
                var s = await _context.TblStudents
                    .Where(x => x.StudentId == record.StudentId)
                    .Select(x => new { x.StudentId, DisplayName = x.StudentName + " | Roll: " + x.RollNo })
                    .FirstOrDefaultAsync();
                ViewBag.SelectedStudent = s;
            }

            return PartialView("_StudentFeeOverrideModal", record);
        }

        // POST: StudentFeeOverride/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudentFeeOverride model)
        {
            try
            {
                // Duplicate check — same student + feetype
                bool isDuplicate = await _context.TblStudentFeeOverrides
                    .AnyAsync(s => s.StudentId == model.StudentId
                               && s.FeeTypeId == model.FeeTypeId
                               && s.IsActive == true
                               && s.Id != id);

                if (isDuplicate)
                    return Json(new { success = false, message = "Override already exists for this Student + Fee Type combination!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblStudentFeeOverrides.Add(model);
                }
                else
                {
                    var existing = await _context.TblStudentFeeOverrides.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Record not found!" });

                    existing.StudentId = model.StudentId;
                    existing.FeeTypeId = model.FeeTypeId;
                    existing.Amount = model.Amount;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Fee Override saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving record! " + ex.Message });
            }
        }

        // POST: StudentFeeOverride/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.TblStudentFeeOverrides.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Fee Override deleted successfully!" });
        }

        // GET: StudentFeeOverride/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var record = await _context.TblStudentFeeOverrides
                .Include(s => s.Student)
                .Include(s => s.FeeType)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (record == null) return NotFound();

            return PartialView("_StudentFeeOverrideViewModal", record);
        }

        private async Task LoadDropdowns()
        {
            // Students ab Select2 AJAX se load honge — yahan sirf Regular FeeTypes chahiye
            // Optional fees override nahi hoti, isliye FeeCategory != "Optional" filter
            ViewBag.FeeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true && f.FeeCategory != "Optional")
                .OrderBy(f => f.FeeName)
                .Select(f => new
                {
                    f.FeeTypeId,
                    f.FeeName,
                    IsRecurring = f.IsRecurring == true ? "Monthly" : "One Time"
                })
                .ToListAsync();
        }
    }
}
