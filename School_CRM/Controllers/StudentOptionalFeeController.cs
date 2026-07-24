using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudentOptionalFeeController : Controller
    {
        private readonly LibmanagementContext _context;

        public StudentOptionalFeeController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: StudentOptionalFee/SearchStudents - Select2 AJAX
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
            var data  = await query
                .OrderBy(ss => ss.Student!.StudentName)
                .Skip((page - 1) * pageSize).Take(pageSize)
                .Select(ss => new {
                    id          = ss.Student!.StudentId,
                    text        = ss.Student.StudentName + " | Roll: " + ss.Student.RollNo,
                    name        = ss.Student.StudentName  ?? "-",
                    roll        = ss.Student.RollNo       ?? "-",
                    className   = ss.Class   != null ? ss.Class.ClassName     : "-",
                    sectionName = ss.Section != null ? ss.Section.SectionName : "-",
                    sessionId   = ss.SessionId,
                    sessionName = ss.Session != null ? ss.Session.SessionName : "-"
                }).ToListAsync();

            return Json(new { results = data, pagination = new { more = (page * pageSize) < total } });
        }

        // GET: StudentOptionalFee/GetAll
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblStudentOptionalFees
                .Where(x => x.IsActive)
                .Include(x => x.Student)
                .Include(x => x.Session)
                .Include(x => x.FeeType)
                .Select(x => new
                {
                    x.Id,
                    StudentName = x.Student.StudentName ?? "-",
                    RollNo      = x.Student.RollNo      ?? "-",
                    SessionName = x.Session.SessionName ?? "-",
                    FeeName     = x.FeeType.FeeName     ?? "-",
                    IsRecurring = x.FeeType.IsRecurring == true ? "Monthly" : "One Time",
                    Amount      = x.Amount,
                    x.Remarks,
                    Status      = x.IsActive ? "Active" : "Inactive",
                    CreatedDate = x.CreatedDate.HasValue
                                    ? x.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: StudentOptionalFee/Index
        public IActionResult Index() => View();

        // GET: StudentOptionalFee/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();

            if (id == 0)
                return PartialView("_StudentOptionalFeeModal",
                    new TblStudentOptionalFee { IsActive = true });

            var record = await _context.TblStudentOptionalFees.FindAsync(id);
            if (record == null) return NotFound();

            // Edit mode: selected student naam
            if (record.StudentId > 0)
            {
                var s = await _context.TblStudents
                    .Where(x => x.StudentId == record.StudentId)
                    .Select(x => new { x.StudentId, DisplayName = x.StudentName + " | Roll: " + x.RollNo })
                    .FirstOrDefaultAsync();
                ViewBag.SelectedStudent = s;
            }

            return PartialView("_StudentOptionalFeeModal", record);
        }

        // POST: StudentOptionalFee/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudentOptionalFee model)
        {
            try
            {
                // Duplicate check: same Student + Session + FeeType
                bool isDuplicate = await _context.TblStudentOptionalFees
                    .AnyAsync(x => x.StudentId  == model.StudentId
                               && x.SessionId  == model.SessionId
                               && x.FeeTypeId  == model.FeeTypeId
                               && x.IsActive
                               && x.Id         != id);

                if (isDuplicate)
                    return Json(new { success = false,
                        message = "Is student ke liye yeh optional fee is session mein already exist karti hai!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy   = 1;
                    _context.TblStudentOptionalFees.Add(model);
                }
                else
                {
                    var existing = await _context.TblStudentOptionalFees.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Record not found!" });

                    existing.StudentId = model.StudentId;
                    existing.SessionId = model.SessionId;
                    existing.FeeTypeId = model.FeeTypeId;
                    existing.Amount    = model.Amount;
                    existing.Remarks   = model.Remarks;
                    existing.IsActive  = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy   = 1;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Optional fee saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: StudentOptionalFee/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.TblStudentOptionalFees.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsActive    = false;
            record.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Record deleted successfully!" });
        }

        // GET: StudentOptionalFee/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var record = await _context.TblStudentOptionalFees
                .Include(x => x.Student)
                .Include(x => x.Session)
                .Include(x => x.FeeType)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (record == null) return NotFound();
            return PartialView("_StudentOptionalFeeViewModal", record);
        }

        // GET: StudentOptionalFee/GetByStudent — AJAX: student + session select pe call
        [HttpGet]
        public async Task<IActionResult> GetByStudent(int studentId, int sessionId)
        {
            var fees = await _context.TblStudentOptionalFees
                .Where(x => x.StudentId == studentId
                         && x.SessionId == sessionId
                         && x.IsActive)
                .Include(x => x.FeeType)
                .Select(x => new
                {
                    x.FeeTypeId,
                    FeeName     = x.FeeType.FeeName ?? "-",
                    IsRecurring = x.FeeType.IsRecurring == true,
                    x.Amount,
                    x.Remarks
                })
                .ToListAsync();

            return Json(new { success = true, fees });
        }

        // ── Private Helpers ───────────────────────────────────────────────────
        private async Task LoadDropdowns()
        {
            // Students ab Select2 AJAX se load honge

            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            // Sirf Optional category wali fee types dikhao
            ViewBag.FeeTypes = await _context.TblFeeTypes
                .Where(f => f.IsActive == true && f.FeeCategory == "Optional")
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
