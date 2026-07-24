using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class StudentDueController : Controller
    {
        private readonly LibmanagementContext _context;

        public StudentDueController(LibmanagementContext context)
        {
            _context = context;
        }

        // GET: StudentDue/Index
        public IActionResult Index()
        {
            return View();
        }

        // GET: StudentDue/GetAll - AJAX Grid Data
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblStudentDues
                .Where(d => d.IsActive == true)
                .Include(d => d.Student)
                .Include(d => d.Session)
                .Select(d => new
                {
                    d.Id,
                    StudentName = d.Student != null ? d.Student.StudentName : "-",
                    RollNo = d.Student != null ? d.Student.RollNo : "-",
                    SessionName = d.Session != null ? d.Session.SessionName : "-",
                    MonthName = d.Month != null
                                    ? System.Globalization.CultureInfo.CurrentCulture
                                        .DateTimeFormat.GetMonthName(d.Month.Value)
                                    : "-",
                    d.Month,
                    d.Year,
                    MonthYear = d.Month != null && d.Year != null
                                    ? System.Globalization.CultureInfo.CurrentCulture
                                        .DateTimeFormat.GetMonthName(d.Month.Value) + " " + d.Year
                                    : "-",
                    TotalDue = d.TotalDue ?? 0,
                    PaidAmount = d.PaidAmount ?? 0,
                    RemainingDue = (d.TotalDue ?? 0) - (d.PaidAmount ?? 0),
                    d.DueType,
                    DueDate = d.DueDate.HasValue
                                    ? d.DueDate.Value.ToString("dd-MM-yyyy") : "-",
                    IsSettled = d.IsSettled == true ? "Settled" : "Pending",
                    SettledDate = d.SettledDate.HasValue
                                    ? d.SettledDate.Value.ToString("dd-MM-yyyy") : "-",
                    d.Remarks,
                    CreatedDate = d.CreatedDate.HasValue
                                    ? d.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(new { data });
        }

        // GET: StudentDue/GetSummary - AJAX Summary Stats
        [HttpGet]
        public async Task<IActionResult> GetSummary(int? sessionId)
        {
            var query = _context.TblStudentDues.Where(d => d.IsActive == true);

            if (sessionId.HasValue && sessionId > 0)
                query = query.Where(d => d.SessionId == sessionId);

            var all = await query.ToListAsync();

            return Json(new
            {
                totalStudents = all.Select(d => d.StudentId).Distinct().Count(),
                totalDue = all.Sum(d => d.TotalDue ?? 0),
                totalPaid = all.Sum(d => d.PaidAmount ?? 0),
                totalRemaining = all.Sum(d => (d.TotalDue ?? 0) - (d.PaidAmount ?? 0)),
                pendingCount = all.Count(d => d.IsSettled != true),
                settledCount = all.Count(d => d.IsSettled == true)
            });
        }

        // GET: StudentDue/CreateOrEdit/5
        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();

            if (id == 0)
                return PartialView("_StudentDueModal", new TblStudentDue
                {
                    IsActive = true,
                    IsSettled = false,
                    Month = DateTime.Now.Month,
                    Year = DateTime.Now.Year
                });

            var record = await _context.TblStudentDues.FindAsync(id);
            if (record == null) return NotFound();

            return PartialView("_StudentDueModal", record);
        }

        // POST: StudentDue/CreateOrEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblStudentDue model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.CreatedBy = 1;
                    _context.TblStudentDues.Add(model);
                }
                else
                {
                    var existing = await _context.TblStudentDues.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Record not found!" });

                    existing.StudentId = model.StudentId;
                    existing.SessionId = model.SessionId;
                    existing.Month = model.Month;
                    existing.Year = model.Year;
                    existing.TotalDue = model.TotalDue;
                    existing.PaidAmount = model.PaidAmount;
                    existing.DueType = model.DueType;
                    existing.DueDate = model.DueDate;
                    existing.IsSettled = model.IsSettled;
                    existing.SettledDate = model.IsSettled == true ? DateTime.Now : null;
                    existing.Remarks = model.Remarks;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                    existing.UpdatedBy = 1;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student Due saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // POST: StudentDue/MarkSettled/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkSettled(int id)
        {
            var record = await _context.TblStudentDues.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsSettled = true;
            record.SettledDate = DateTime.Now;
            record.PaidAmount = record.TotalDue;
            record.UpdatedDate = DateTime.Now;
            record.UpdatedBy = 1;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Due marked as settled!" });
        }

        // POST: StudentDue/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var record = await _context.TblStudentDues.FindAsync(id);
            if (record == null)
                return Json(new { success = false, message = "Record not found!" });

            record.IsActive = false;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Record deleted successfully!" });
        }

        // GET: StudentDue/View/5
        [HttpGet]
        public async Task<IActionResult> View(int id)
        {
            var record = await _context.TblStudentDues
                .Include(d => d.Student)
                .Include(d => d.Session)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (record == null) return NotFound();

            return PartialView("_StudentDueViewModal", record);
        }

        // GET: StudentDue/GetDefaulters - students with pending dues
        [HttpGet]
        public async Task<IActionResult> GetDefaulters(int? sessionId)
        {
            IQueryable<TblStudentDue> query = _context.TblStudentDues
                .Where(d => d.IsActive == true
                         && d.IsSettled != true
                         && (d.TotalDue ?? 0) > (d.PaidAmount ?? 0))
                .Include(d => d.Student)
                .Include(d => d.Session);

            // Fix 1: sessionId.Value use karo direct sessionId ki jagah
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(d => d.SessionId == sid);
            }

            var defaulters = await query
                // Fix 2: Student null ho sakta hai — null check add karo
                .Where(d => d.Student != null)
                .GroupBy(d => new
                {
                    d.StudentId,
                    StudentName = d.Student!.StudentName,
                    RollNo = d.Student!.RollNo
                })
                .Select(g => new
                {
                    g.Key.StudentName,
                    g.Key.RollNo,
                    TotalPending = g.Sum(d => (d.TotalDue ?? 0) - (d.PaidAmount ?? 0)),
                    MonthsCount = g.Count()
                })
                .OrderByDescending(d => d.TotalPending)
                .Take(10)
                .ToListAsync();

            return Json(new { success = true, defaulters });
        }
        // ── Private Helper ────────────────────────────────────────────────────
        private async Task LoadDropdowns()
        {
            ViewBag.Students = await _context.TblStudents
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.StudentName)
                .Select(s => new
                {
                    s.StudentId,
                    DisplayName = s.StudentName + " | Roll: " + s.RollNo
                })
                .ToListAsync();

            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            ViewBag.Months = Enumerable.Range(1, 12).Select(m => new
            {
                Value = m,
                Text = System.Globalization.CultureInfo.CurrentCulture
                            .DateTimeFormat.GetMonthName(m)
            }).ToList();

            ViewBag.Years = Enumerable.Range(DateTime.Now.Year - 2, 5).Select(y => new
            {
                Value = y,
                Text = y.ToString()
            }).ToList();

            ViewBag.DueTypes = new[]
            {
                new { Value = "Tuition",   Text = "Tuition"   },
                new { Value = "Exam",      Text = "Exam"      },
                new { Value = "Transport", Text = "Transport" },
                new { Value = "Library",   Text = "Library"   },
                new { Value = "Other",     Text = "Other"     }
            };
        }
    }
}