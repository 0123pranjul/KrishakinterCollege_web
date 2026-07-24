using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Reports/[action]")]
    public class SchoolReportController : Controller
    {
        private readonly LibmanagementContext _db;

        public SchoolReportController(LibmanagementContext db) => _db = db;

        // ── REPORT HUB ────────────────────────────────────────────────────
        [HttpGet]
        [Route("/Reports")]
        public IActionResult Index() => View();

        // ================================================================
        // ATTENDANCE REPORTS
        // ================================================================

        // ── STUDENT ATTENDANCE ────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> StudentAttendance(
            int? studentId, int? classId, int? sectionId,
            int? month, int? year, int? sessionId)
        {
            await LoadAttendanceDropdowns();

            if (!studentId.HasValue && !classId.HasValue)
                return View(new StudentAttendanceReportVm());

            var m   = month ?? DateTime.Today.Month;
            var y   = year  ?? DateTime.Today.Year;
            var from = new DateOnly(y, m, 1);
            var to   = from.AddMonths(1).AddDays(-1);

            var query = _db.TblStudentAttendances
                .Include(a => a.Student)
                .Where(a => a.AttendanceDate >= from && a.AttendanceDate <= to
                         && a.IsActive == true);

            if (studentId.HasValue) query = query.Where(a => a.StudentId == studentId);
            if (classId.HasValue)   query = query.Where(a => a.ClassId == classId);
            if (sectionId.HasValue) query = query.Where(a => a.SectionId == sectionId);
            if (sessionId.HasValue) query = query.Where(a => a.SessionId == sessionId);

            var records = await query.OrderBy(a => a.AttendanceDate).ToListAsync();

            var vm = new StudentAttendanceReportVm
            {
                StudentId  = studentId,
                ClassId    = classId,
                SectionId  = sectionId,
                Month      = m,
                Year       = y,
                SessionId  = sessionId,
                Records    = records,
                Present    = records.Count(r => r.Status == "Present"),
                Absent     = records.Count(r => r.Status == "Absent"),
                Late       = records.Count(r => r.Status == "Late"),
                Total      = records.Count,
                Percentage = records.Count > 0
                    ? Math.Round(records.Count(r => r.Status == "Present") * 100m / records.Count, 2)
                    : 0
            };

            return View(vm);
        }

        // ── CLASS ATTENDANCE ──────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ClassAttendance(
            int? classId, int? sectionId, int? sessionId,
            int? month, int? year)
        {
            await LoadAttendanceDropdowns();

            if (!classId.HasValue)
                return View(new List<ClassAttendanceRowVm>());

            var m    = month ?? DateTime.Today.Month;
            var y    = year  ?? DateTime.Today.Year;
            var from = new DateOnly(y, m, 1);
            var to   = from.AddMonths(1).AddDays(-1);

            var query = _db.TblStudentAttendances
                .Include(a => a.Student)
                .Where(a => a.ClassId == classId
                         && a.AttendanceDate >= from
                         && a.AttendanceDate <= to
                         && a.IsActive == true);

            if (sectionId.HasValue) query = query.Where(a => a.SectionId == sectionId);
            if (sessionId.HasValue) query = query.Where(a => a.SessionId == sessionId);

            var records = await query.ToListAsync();

            var rows = records
                .GroupBy(a => new { a.StudentId, Name = a.Student.StudentName ?? "" })
                .Select(g => new ClassAttendanceRowVm
                {
                    StudentId  = g.Key.StudentId,
                    StudentName = g.Key.Name,
                    Present    = g.Count(r => r.Status == "Present"),
                    Absent     = g.Count(r => r.Status == "Absent"),
                    Late       = g.Count(r => r.Status == "Late"),
                    Total      = g.Count(),
                    Percentage = g.Count() > 0
                        ? Math.Round(g.Count(r => r.Status == "Present") * 100m / g.Count(), 2)
                        : 0
                })
                .OrderBy(r => r.StudentName)
                .ToList();

            ViewBag.Month   = m;
            ViewBag.Year    = y;
            ViewBag.ClassId = classId;
            return View(rows);
        }

        // ================================================================
        // ACADEMIC REPORTS
        // ================================================================

        // ── CLASS PERFORMANCE ─────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> ClassPerformance(
            int? classId, int? sectionId, int? examId, int? sessionId)
        {
            await LoadAcademicDropdowns();

            if (!examId.HasValue || !classId.HasValue)
                return View(new ClassPerformanceVm());

            // Get all marks for this exam and class
            var marks = await _db.TblExamMarks
                .Include(m => m.Student)
                .Include(m => m.Subject)
                .Include(m => m.Exam)
                .Where(m => m.ExamId == examId
                         && m.IsActive == true
                         && m.IsAbsent != true)
                .ToListAsync();

            // Filter by class via student sessions
            var classStudentIds = await _db.TblStudentSessions
                .Where(ss => ss.ClassId == classId
                          && (sectionId == null || ss.SectionId == sectionId)
                          && ss.IsActive == true)
                .Select(ss => ss.StudentId)
                .ToListAsync();

            marks = marks.Where(m => classStudentIds.Contains(m.StudentId)).ToList();

            // Subject-wise stats
            var subjectStats = marks
                .GroupBy(m => m.Subject.SubjectName)
                .Select(g => new SubjectStatVm
                {
                    SubjectName = g.Key,
                    Average     = g.Any() ? Math.Round(g.Average(m => m.MarksObtained ?? 0), 1) : 0,
                    Highest     = g.Any() ? g.Max(m => m.MarksObtained ?? 0) : 0,
                    Lowest      = g.Any() ? g.Min(m => m.MarksObtained ?? 0) : 0,
                    TotalStudents = g.Count()
                })
                .OrderBy(s => s.SubjectName)
                .ToList();

            // Top 5 students by total marks
            var topStudents = marks
                .GroupBy(m => new { m.StudentId, Name = m.Student.StudentName ?? "" })
                .Select(g => new TopStudentVm
                {
                    StudentId   = g.Key.StudentId,
                    StudentName = g.Key.Name,
                    TotalMarks  = g.Sum(m => m.MarksObtained ?? 0)
                })
                .OrderByDescending(s => s.TotalMarks)
                .Take(5)
                .ToList();

            var vm = new ClassPerformanceVm
            {
                ExamId       = examId,
                ClassId      = classId,
                SectionId    = sectionId,
                ExamName     = marks.FirstOrDefault()?.Exam.ExamName ?? "",
                SubjectStats = subjectStats,
                TopStudents  = topStudents
            };

            return View(vm);
        }

        // ================================================================
        // FEE REPORTS
        // ================================================================

        // ── PENDING FEES ──────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> PendingFees(
            int? classId, int? sectionId, int? sessionId,
            int? month, int? year)
        {
            await LoadFeeDropdowns();

            var query = _db.TblFeeCollections
                .Include(f => f.Student)
                .Where(f => f.IsActive == true
                         && f.TotalAmount > f.PaidAmount);

            if (sessionId.HasValue) query = query.Where(f => f.SessionId == sessionId);
            if (month.HasValue)     query = query.Where(f => f.Month == month);
            if (year.HasValue)      query = query.Where(f => f.Year == year);

            var records = await query.ToListAsync();

            // Filter by class via student sessions
            if (classId.HasValue)
            {
                var classStudentIds = await _db.TblStudentSessions
                    .Where(ss => ss.ClassId == classId
                              && (sectionId == null || ss.SectionId == sectionId)
                              && ss.IsActive == true)
                    .Select(ss => ss.StudentId)
                    .ToListAsync();
                records = records.Where(f => f.StudentId.HasValue && classStudentIds.Contains(f.StudentId.Value)).ToList();
            }

            var rows = records
                .GroupBy(f => new { f.StudentId, Name = f.Student?.StudentName ?? "" })
                .Select(g => new PendingFeeRowVm
                {
                    StudentId   = g.Key.StudentId ?? 0,
                    StudentName = g.Key.Name,
                    TotalDue    = g.Sum(f => (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0)),
                    OldestMonth = g.Min(f => f.Month ?? 0),
                    OldestYear  = g.Min(f => f.Year ?? 0)
                })
                .Where(r => r.TotalDue > 0)
                .OrderByDescending(r => r.TotalDue)
                .ToList();

            ViewBag.GrandTotal = rows.Sum(r => r.TotalDue);
            ViewBag.ClassId    = classId;
            ViewBag.SessionId  = sessionId;
            return View(rows);
        }

        // ── FEE COLLECTION ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> FeeCollection(
            DateTime? fromDate, DateTime? toDate,
            int? classId, string? paymentMode, int? sessionId)
        {
            await LoadFeeDropdowns();

            var from = fromDate ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var to   = toDate   ?? DateTime.Today;

            var query = _db.TblFeeCollections
                .Include(f => f.Student)
                .Where(f => f.IsActive == true
                         && f.PaymentDate.HasValue
                         && f.PaymentDate.Value.Date >= from.Date
                         && f.PaymentDate.Value.Date <= to.Date
                         && (f.PaidAmount ?? 0) > 0);

            if (!string.IsNullOrEmpty(paymentMode))
                query = query.Where(f => f.PaymentMode == paymentMode);
            if (sessionId.HasValue)
                query = query.Where(f => f.SessionId == sessionId);

            var records = await query.OrderByDescending(f => f.PaymentDate).ToListAsync();

            // Daily summary
            var dailySummary = records
                .GroupBy(f => f.PaymentDate!.Value.Date)
                .Select(g => new DailyFeeVm
                {
                    Date        = g.Key,
                    TotalAmount = g.Sum(f => f.PaidAmount ?? 0),
                    Cash        = g.Where(f => f.PaymentMode == "Cash").Sum(f => f.PaidAmount ?? 0),
                    Online      = g.Where(f => f.PaymentMode == "Online").Sum(f => f.PaidAmount ?? 0),
                    UPI         = g.Where(f => f.PaymentMode == "UPI").Sum(f => f.PaidAmount ?? 0),
                    Count       = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            ViewBag.FromDate    = from;
            ViewBag.ToDate      = to;
            ViewBag.PaymentMode = paymentMode;
            ViewBag.GrandTotal  = records.Sum(f => f.PaidAmount ?? 0);
            ViewBag.Records     = records;
            return View(dailySummary);
        }

        // ================================================================
        // LIBRARY REPORTS
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> Library(
            string tab = "issued",
            DateTime? fromDate = null, DateTime? toDate = null,
            string? userType = null)
        {
            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to   = toDate   ?? DateTime.Today;

            ViewBag.Tab      = tab;
            ViewBag.FromDate = from;
            ViewBag.ToDate   = to;
            ViewBag.UserType = userType;

            switch (tab)
            {
                case "overdue":
                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var overdue = await _db.LibIssueTransactions
                        .Include(t => t.Copy).ThenInclude(c => c.Book)
                        .Where(t => !t.IsReturned && t.DueDate < today)
                        .OrderBy(t => t.DueDate)
                        .ToListAsync();
                    if (!string.IsNullOrEmpty(userType))
                        overdue = overdue.Where(t => t.UserType == userType).ToList();
                    return View(overdue.Cast<object>().ToList());

                case "fine":
                    var fineQuery = _db.LibFinePayments
                        .Where(p => p.PaymentDate.Date >= from.Date && p.PaymentDate.Date <= to.Date);
                    var fines = await fineQuery.OrderByDescending(p => p.PaymentDate).ToListAsync();
                    ViewBag.TotalFine = fines.Sum(f => f.AmountPaid);
                    return View(fines.Cast<object>().ToList());

                case "lost":
                    var lost = await _db.LibIssueTransactions
                        .Include(t => t.Copy).ThenInclude(c => c.Book)
                        .Where(t => t.TransactionStatus == "Lost" || t.TransactionStatus == "Damaged")
                        .OrderByDescending(t => t.CreatedAt)
                        .ToListAsync();
                    return View(lost.Cast<object>().ToList());

                default: // issued
                    var issued = await _db.LibIssueTransactions
                        .Include(t => t.Copy).ThenInclude(c => c.Book)
                        .Where(t => !t.IsReturned)
                        .OrderBy(t => t.DueDate)
                        .ToListAsync();
                    if (!string.IsNullOrEmpty(userType))
                        issued = issued.Where(t => t.UserType == userType).ToList();
                    return View(issued.Cast<object>().ToList());
            }
        }

        // ================================================================
        // INVENTORY REPORTS
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> Inventory(
            string tab = "stock",
            int? categoryId = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to   = toDate   ?? DateTime.Today;

            ViewBag.Tab        = tab;
            ViewBag.FromDate   = from;
            ViewBag.ToDate     = to;
            ViewBag.CategoryId = categoryId;
            ViewBag.Categories = new SelectList(
                await _db.InvCategories.Where(c => c.IsActive).ToListAsync(),
                "CategoryId", "CategoryName");

            switch (tab)
            {
                case "lowstock":
                    var lowStock = await _db.InvProducts
                        .Include(p => p.Category)
                        .Where(p => p.IsActive && p.CurrentStock <= p.ReorderLevel)
                        .OrderBy(p => p.CurrentStock)
                        .ToListAsync();
                    if (categoryId.HasValue)
                        lowStock = lowStock.Where(p => p.CategoryId == categoryId).ToList();
                    return View(lowStock.Cast<object>().ToList());

                case "grn":
                    var grnQuery = _db.InvStockReceipts
                        .Include(r => r.Supplier)
                        .Where(r => r.ReceiptDate >= DateOnly.FromDateTime(from)
                                 && r.ReceiptDate <= DateOnly.FromDateTime(to));
                    var grns = await grnQuery.OrderByDescending(r => r.ReceiptDate).ToListAsync();
                    ViewBag.TotalGRN = grns.Sum(r => r.TotalAmount);
                    return View(grns.Cast<object>().ToList());

                case "sales":
                    var salesQuery = _db.InvSaleTransactions
                        .Where(s => s.SaleDate >= DateOnly.FromDateTime(from)
                                 && s.SaleDate <= DateOnly.FromDateTime(to));
                    var sales = await salesQuery.OrderByDescending(s => s.SaleDate).ToListAsync();
                    ViewBag.TotalSales = sales.Sum(s => s.TotalAmount);
                    return View(sales.Cast<object>().ToList());

                default: // stock
                    var stockQuery = _db.InvProducts
                        .Include(p => p.Category)
                        .Include(p => p.Unit)
                        .Where(p => p.IsActive);
                    if (categoryId.HasValue)
                        stockQuery = stockQuery.Where(p => p.CategoryId == categoryId);
                    var stock = await stockQuery.OrderBy(p => p.ProductName).ToListAsync();
                    ViewBag.TotalStockValue = stock.Sum(p => p.CurrentStock * p.CostPrice);
                    return View(stock.Cast<object>().ToList());
            }
        }

        // ================================================================
        // ASSET REPORTS
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> Assets(
            string tab = "allocation",
            int? categoryId = null,
            DateTime? fromDate = null, DateTime? toDate = null)
        {
            var from = fromDate ?? DateTime.Today.AddMonths(-3);
            var to   = toDate   ?? DateTime.Today;

            ViewBag.Tab      = tab;
            ViewBag.FromDate = from;
            ViewBag.ToDate   = to;

            switch (tab)
            {
                case "repair":
                    var repair = await _db.AsmMaintenanceLogs
                        .Include(m => m.Unit).ThenInclude(u => u.Asset)
                        .Where(m => m.Status == "InProgress" || m.Status == "Pending")
                        .OrderBy(m => m.StartDate)
                        .ToListAsync();
                    return View(repair.Cast<object>().ToList());

                case "disposed":
                    var disposedQuery = _db.AsmDisposalLogs
                        .Include(d => d.Unit).ThenInclude(u => u.Asset)
                        .Where(d => d.DisposalDate >= DateOnly.FromDateTime(from)
                                 && d.DisposalDate <= DateOnly.FromDateTime(to));
                    var disposed = await disposedQuery.OrderByDescending(d => d.DisposalDate).ToListAsync();
                    return View(disposed.Cast<object>().ToList());

                default: // allocation
                    var allocated = await _db.AsmAssetUnits
                        .Include(u => u.Asset).ThenInclude(a => a.Category)
                        .Include(u => u.CurrentLocation)
                        .Where(u => !u.IsAvailable && u.IsActive)
                        .OrderBy(u => u.Asset.AssetName)
                        .ToListAsync();
                    return View(allocated.Cast<object>().ToList());
            }
        }

        // ================================================================
        // COMMUNICATION REPORTS
        // ================================================================

        [HttpGet]
        public async Task<IActionResult> Communication(
            string tab = "notifications",
            DateTime? fromDate = null, DateTime? toDate = null,
            string? notificationType = null, int? announcementId = null)
        {
            var from = fromDate ?? DateTime.Today.AddMonths(-1);
            var to   = toDate   ?? DateTime.Today;

            ViewBag.Tab              = tab;
            ViewBag.FromDate         = from;
            ViewBag.ToDate           = to;
            ViewBag.NotificationType = notificationType;
            ViewBag.AnnouncementId   = announcementId;
            ViewBag.Announcements    = new SelectList(
                await _db.CommAnnouncements.Where(a => a.IsPublished).OrderByDescending(a => a.CreatedAt).ToListAsync(),
                "AnnouncementId", "Title");

            if (tab == "reads" && announcementId.HasValue)
            {
                var reads = await _db.CommAnnouncementReads
                    .Include(r => r.Announcement)
                    .Where(r => r.AnnouncementId == announcementId)
                    .OrderByDescending(r => r.ReadAt)
                    .ToListAsync();
                ViewBag.ReadCount = reads.Count;
                return View(reads.Cast<object>().ToList());
            }

            // Notification stats
            var notiQuery = _db.CommNotifications
                .Where(n => n.CreatedAt.Date >= from.Date && n.CreatedAt.Date <= to.Date);
            if (!string.IsNullOrEmpty(notificationType))
                notiQuery = notiQuery.Where(n => n.NotificationType == notificationType);

            var stats = await notiQuery
                .GroupBy(n => n.NotificationType)
                .Select(g => new NotiStatVm
                {
                    NotificationType = g.Key,
                    Total            = g.Count(),
                    ReadCount        = g.Count(n => n.IsRead),
                    ReadRate         = g.Count() > 0
                        ? Math.Round(g.Count(n => n.IsRead) * 100m / g.Count(), 1)
                        : 0
                })
                .OrderByDescending(s => s.Total)
                .ToListAsync();

            return View(stats.Cast<object>().ToList());
        }

        // ================================================================
        // HELPERS
        // ================================================================

        private async Task LoadAttendanceDropdowns()
        {
            ViewBag.Classes  = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
            ViewBag.Sections = new SelectList(await _db.TblSections.Where(s => s.IsActive == true).ToListAsync(), "SectionId", "SectionName");
            ViewBag.Sessions = new SelectList(await _db.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync(), "SessionId", "SessionName");
            ViewBag.Students = new SelectList(await _db.TblStudents.Where(s => s.IsActive == true).OrderBy(s => s.StudentName).ToListAsync(), "StudentId", "StudentName");
        }

        private async Task LoadAcademicDropdowns()
        {
            ViewBag.Classes  = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
            ViewBag.Sections = new SelectList(await _db.TblSections.Where(s => s.IsActive == true).ToListAsync(), "SectionId", "SectionName");
            ViewBag.Sessions = new SelectList(await _db.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync(), "SessionId", "SessionName");
            ViewBag.Exams    = new SelectList(await _db.TblExams.Where(e => e.IsActive == true).ToListAsync(), "ExamId", "ExamName");
        }

        private async Task LoadFeeDropdowns()
        {
            ViewBag.Classes  = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
            ViewBag.Sections = new SelectList(await _db.TblSections.Where(s => s.IsActive == true).ToListAsync(), "SectionId", "SectionName");
            ViewBag.Sessions = new SelectList(await _db.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync(), "SessionId", "SessionName");
        }
    }
}
