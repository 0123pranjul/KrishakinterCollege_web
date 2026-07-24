using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudentExtraChargeController : Controller
    {
        private readonly LibmanagementContext _context;

        public StudentExtraChargeController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: StudentExtraCharge/SearchStudents - Select2 AJAX
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

            var total = await query.Select(ss => ss.StudentId).Distinct().CountAsync();

            var data = await query
                .OrderBy(ss => ss.Student!.StudentName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(ss => new {
                    id          = ss.Student!.StudentId,
                    text        = ss.Student.StudentName + " | Roll: " + ss.Student.RollNo,
                    name        = ss.Student.StudentName  ?? "-",
                    roll        = ss.Student.RollNo       ?? "-",
                    className   = ss.Class   != null ? ss.Class.ClassName     : "-",
                    sectionName = ss.Section != null ? ss.Section.SectionName : "-",
                    sessionId   = ss.SessionId,
                    sessionName = ss.Session != null ? ss.Session.SessionName : "-"
                })
                .ToListAsync();

            return Json(new {
                results    = data,
                pagination = new { more = (page * pageSize) < total }
            });
        }

        // GET: StudentExtraCharge/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: StudentExtraCharge/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblStudentExtraCharges
                .Where(s => s.IsActive == true)
                .Include(s => s.Student)
                .Include(s => s.Session)
                .Include(s => s.FeeType)
                .Select(s => new
                {
                    s.Id,
                    StudentName = s.Student != null ? s.Student.StudentName : "-",
                    RollNo = s.Student != null ? s.Student.RollNo : "-",
                    SessionName = s.Session != null ? s.Session.SessionName : "-",
                    FeeName = s.FeeType != null ? s.FeeType.FeeName : "-",
                    Amount = s.Amount != null ? "₹" + s.Amount.Value.ToString("0.00") : "₹0.00",
                    s.Reason,
                    ChargeDate = s.ChargeDate.HasValue ? s.ChargeDate.Value.ToString("dd-MM-yyyy") : "-",
                    IsPaid = s.IsPaid == true ? "Paid" : "Pending",
                    Status = s.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = s.CreatedDate.HasValue ? s.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: StudentExtraCharge/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();

            if (id == 0)
                return PartialView("_StudentExtraChargeModal", new TblStudentExtraCharge
                {
                    IsActive = true,
                    IsPaid = false,
                    ChargeDate = DateOnly.FromDateTime(DateTime.Today)
                });

            var record = await _context.TblStudentExtraCharges.FindAsync(id);
            if (record == null) return NotFound();

            // Edit mode: selected student ka naam fetch karo
            if (record.StudentId.HasValue)
            {
                var s = await _context.TblStudents
                    .Where(x => x.StudentId == record.StudentId)
                    .Select(x => new { x.StudentId, DisplayName = x.StudentName + " | Roll: " + x.RollNo })
                    .FirstOrDefaultAsync();
                ViewBag.SelectedStudent = s;
            }

            return PartialView("_StudentExtraChargeModal", record);
        }

        // POST: StudentExtraCharge/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudentExtraCharge model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = 1; // Replace with logged-in user ID
                    _context.TblStudentExtraCharges.Add(model);
                }
                else
                {
                    var existing = await _context.TblStudentExtraCharges.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Record not found!" });

                    existing.StudentId = model.StudentId;
                    existing.SessionId = model.SessionId;
                    existing.FeeTypeId = model.FeeTypeId;
                    existing.Amount = model.Amount;
                    existing.Reason = model.Reason;
                    existing.ChargeDate = model.ChargeDate;
                    existing.IsPaid = model.IsPaid;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Extra Charge saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error saving record! " + ex.Message });
            }
        }

        // POST: StudentExtraCharge/MarkPaid/5 - Quick action
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkPaid(int id)
        {
            var record = await _context.TblStudentExtraCharges.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsPaid = true;
            record.UpdatedDate = DateTime.Now;
            record.UpdatedBy = 1;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Charge marked as paid!" });
        }

        // POST: StudentExtraCharge/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.TblStudentExtraCharges.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Extra Charge deleted successfully!" });
        }

        // GET: StudentExtraCharge/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var record = await _context.TblStudentExtraCharges
                .Include(s => s.Student)
                .Include(s => s.Session)
                .Include(s => s.FeeType)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (record == null) return NotFound();

            return PartialView("_StudentExtraChargeViewModal", record);
        }

        private async Task LoadDropdowns()
        {
            // Students ab Select2 AJAX se load honge
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            ViewBag.FeeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true)
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
