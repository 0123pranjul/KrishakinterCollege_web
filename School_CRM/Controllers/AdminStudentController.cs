using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class AdminStudentController : Controller
    {
        private readonly LibmanagementContext _context;

        // ── Admin Cookie Check ────────────────────────────────────────────────
        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";

        public AdminStudentController(LibmanagementContext context)
        {
            _context = context;
        }

        // ── GET: AdminStudent/Index ───────────────────────────────────────────
        public IActionResult Index()
        {
            if (!IsAdmin)
                return RedirectToAction("Login", "Account");

            return View();
        }

        // ── GET: AdminStudent/GetAllStudents - Grid Data ──────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAllStudents(int? sessionId, int? classId, int? sectionId, string? status)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized" });

            var query = _context.TblStudentSessions
                .Where(ss => ss.IsActive == true)
                .Include(ss => ss.Student)
                .Include(ss => ss.Session)
                .Include(ss => ss.Class)
                .Include(ss => ss.Section)
                .AsQueryable();

            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(ss => ss.SessionId == sid);
            }
            if (classId.HasValue && classId.Value > 0)
            {
                int cid = classId.Value;
                query = query.Where(ss => ss.ClassId == cid);
            }
            if (sectionId.HasValue && sectionId.Value > 0)
            {
                int secid = sectionId.Value;
                query = query.Where(ss => ss.SectionId == secid);
            }
            if (!string.IsNullOrEmpty(status))
            {
                bool isActive = status == "active";
                query = query.Where(ss => ss.Student!.IsActive == isActive);
            }

            var data = await query
                .Select(ss => new
                {
                    StudentId = ss.Student != null ? ss.Student.StudentId : 0,
                    StudentName = ss.Student != null ? ss.Student.StudentName : "-",
                    RollNo = ss.Student != null ? ss.Student.RollNo : "-",
                    SessionName = ss.Session != null ? ss.Session.SessionName : "-",
                    ClassName = ss.Class != null ? ss.Class.ClassName : "-",
                    SectionName = ss.Section != null ? ss.Section.SectionName : "-",
                    IsActive = ss.Student != null && ss.Student.IsActive == true ? "Active" : "Inactive",
                    MappingId = ss.Id
                })
                .ToListAsync();

            return Json(new { data });
        }

        // ── GET: AdminStudent/GetStudentDetail/5 - Full Profile ───────────────
        [HttpGet]
        public async Task<IActionResult> GetStudentDetail(int studentId, int? sessionId)
        {
            if (!IsAdmin)
                return Json(new { success = false, message = "Unauthorized" });

            var student = await _context.TblStudents
                .FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (student == null)
                return Json(new { success = false, message = "Student not found!" });

            // ── Session Mappings ──────────────────────────────────────────────
            var sessions = await _context.TblStudentSessions
                .Where(ss => ss.StudentId == studentId && ss.IsActive == true)
                .Include(ss => ss.Session)
                .Include(ss => ss.Class)
                .Include(ss => ss.Section)
                .Select(ss => new
                {
                    ss.Id,
                    SessionName = ss.Session != null ? ss.Session.SessionName : "-",
                    ClassName = ss.Class != null ? ss.Class.ClassName : "-",
                    SectionName = ss.Section != null ? ss.Section.SectionName : "-",
                    ss.SessionId
                })
                .ToListAsync();

            // ── Fee Collections ───────────────────────────────────────────────
            var feeCollections = await _context.TblFeeCollections
                .Where(f => f.StudentId == studentId && f.IsActive == true)
                .Include(f => f.Session)
                .Include(f => f.TblFeeCollectionDetails).ThenInclude(d => d.FeeType)
                .OrderByDescending(f => f.Year).ThenByDescending(f => f.Month)
                .Select(f => new
                {
                    f.FeeCollectionId,
                    SessionName = f.Session != null ? f.Session.SessionName : "-",
                    MonthYear = f.Month != null && f.Year != null
                                     ? System.Globalization.CultureInfo.CurrentCulture
                                         .DateTimeFormat.GetMonthName(f.Month.Value) + " " + f.Year
                                     : "-",
                    f.Month,
                    f.Year,
                    TotalAmount = f.TotalAmount ?? 0,
                    PaidAmount = f.PaidAmount ?? 0,
                    DiscountAmount = f.DiscountAmount ?? 0,
                    FineAmount = f.FineAmount ?? 0,
                    DueAmount = (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0),
                    PaymentMode = f.PaymentMode ?? "-",
                    PaymentDate = f.PaymentDate.HasValue
                                     ? f.PaymentDate.Value.ToString("dd-MM-yyyy") : "-",
                    Details = f.TblFeeCollectionDetails
                        .Where(d => d.IsActive == true)
                        .Select(d => new
                        {
                            FeeName = d.FeeType != null ? d.FeeType.FeeName : "-",
                            Amount = d.Amount ?? 0
                        }).ToList()
                })
                .ToListAsync();

            // ── Extra Charges ─────────────────────────────────────────────────
            var extraCharges = await _context.TblStudentExtraCharges
                .Where(e => e.StudentId == studentId && e.IsActive == true)
                .Include(e => e.Session)
                .Include(e => e.FeeType)
                .OrderByDescending(e => e.ChargeDate)
                .Select(e => new
                {
                    e.Id,
                    SessionName = e.Session != null ? e.Session.SessionName : "-",
                    FeeName = e.FeeType != null ? e.FeeType.FeeName : "-",
                    Amount = e.Amount ?? 0,
                    e.Reason,
                    ChargeDate = e.ChargeDate.HasValue ? e.ChargeDate.Value.ToString("dd-MM-yyyy") : "-",
                    IsPaid = e.IsPaid == true ? "Paid" : "Pending"
                })
                .ToListAsync();

            // ── Student Dues ──────────────────────────────────────────────────
            var dues = await _context.TblStudentDues
                .Where(d => d.StudentId == studentId && d.IsActive == true)
                .Include(d => d.Session)
                .OrderByDescending(d => d.Year).ThenByDescending(d => d.Month)
                .Select(d => new
                {
                    d.Id,
                    SessionName = d.Session != null ? d.Session.SessionName : "-",
                    MonthYear = d.Month != null && d.Year != null
                                   ? System.Globalization.CultureInfo.CurrentCulture
                                       .DateTimeFormat.GetMonthName(d.Month.Value) + " " + d.Year
                                   : "-",
                    TotalDue = d.TotalDue ?? 0,
                    PaidAmount = d.PaidAmount ?? 0,
                    RemainingDue = (d.TotalDue ?? 0) - (d.PaidAmount ?? 0),
                    d.DueType,
                    DueDate = d.DueDate.HasValue ? d.DueDate.Value.ToString("dd-MM-yyyy") : "-",
                    IsSettled = d.IsSettled == true ? "Settled" : "Pending",
                    d.Remarks
                })
                .ToListAsync();

            // ── Fee Overrides ─────────────────────────────────────────────────
            var overrides = await _context.TblStudentFeeOverrides
                .Where(o => o.StudentId == studentId && o.IsActive == true)
                .Include(o => o.FeeType)
                .Select(o => new
                {
                    FeeName = o.FeeType != null ? o.FeeType.FeeName : "-",
                    Amount = o.Amount ?? 0,
                    IsRecurring = o.FeeType != null && o.FeeType.IsRecurring == true ? "Monthly" : "One Time"
                })
                .ToListAsync();

            // ── Summary ───────────────────────────────────────────────────────
            var summary = new
            {
                totalCollected = feeCollections.Sum(f => f.PaidAmount),
                totalDue = feeCollections.Sum(f => f.DueAmount),
                totalDiscount = feeCollections.Sum(f => f.DiscountAmount),
                totalFine = feeCollections.Sum(f => f.FineAmount),
                totalExtraCharge = extraCharges.Sum(e => e.Amount),
                unpaidExtra = extraCharges.Count(e => e.IsPaid == "Pending"),
                pendingDues = dues.Count(d => d.IsSettled == "Pending"),
                totalRemaining = dues.Sum(d => d.RemainingDue)
            };

            return Json(new
            {
                success = true,
                student = new
                {
                    student.StudentId,
                    student.StudentName,
                    student.RollNo,
                    IsActive = student.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = student.CreatedDate.HasValue
                                  ? student.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                },
                sessions,
                feeCollections,
                extraCharges,
                dues,
                overrides,
                summary
            });
        }

        // ── GET: AdminStudent/GetDropdowns ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDropdowns()
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

            var sections = await _context.TblSections
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName })
                .ToListAsync();

            return Json(new { sessions, classes, sections });
        }
    }
}
