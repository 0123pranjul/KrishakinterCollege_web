using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class FeesDashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public FeesDashboardController(LibmanagementContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // ── Summary Stats ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSummaryStats(string? fromDate, string? toDate, int? sessionId)
        {
            var from = string.IsNullOrEmpty(fromDate) ? DateTime.Now.Date : DateTime.Parse(fromDate).Date;
            var to = string.IsNullOrEmpty(toDate) ? DateTime.Now.Date : DateTime.Parse(toDate).Date.AddDays(1);

            var totalStudents = await _context.TblStudents.CountAsync(s => s.IsActive == true);
            var totalSessions = await _context.TblAcademicSessions.CountAsync(s => s.IsActive == true);
            var totalClasses = await _context.TblClasses.CountAsync(c => c.IsActive == true);
            var totalSections = await _context.TblSections.CountAsync(s => s.IsActive == true);
            var totalFeeTypes = await _context.TblFeeTypes.CountAsync(f => f.IsActive == true);
            var totalMappings = await _context.TblStudentSessions.CountAsync(s => s.IsActive == true);
            var totalStructures = await _context.TblFeeStructures.CountAsync(f => f.IsActive == true);
            var totalOverrides = await _context.TblStudentFeeOverrides.CountAsync(f => f.IsActive == true);
            var totalExtraCharges = await _context.TblStudentExtraCharges.CountAsync(e => e.IsActive == true);
            var unpaidExtraCharges = await _context.TblStudentExtraCharges.CountAsync(e => e.IsActive == true && e.IsPaid == false);

            var collQuery = _context.TblFeeCollections.Where(f => f.IsActive == true);
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                collQuery = collQuery.Where(f => f.SessionId == sid);
            }
            var collAll = await collQuery.ToListAsync();

            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);

            var collToday = collAll.Where(f => f.PaymentDate?.Date == today).ToList();
            var collThisMonth = collAll.Where(f => f.PaymentDate >= monthStart).ToList();
            var collFiltered = collAll.Where(f => f.PaymentDate >= from && f.PaymentDate < to).ToList();

            var dueQuery = _context.TblStudentDues.Where(d => d.IsActive == true);
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                dueQuery = dueQuery.Where(d => d.SessionId == sid);
            }
            var dues = await dueQuery.ToListAsync();

            return Json(new
            {
                totalStudents,
                totalSessions,
                totalClasses,
                totalSections,
                totalFeeTypes,
                totalMappings,
                totalStructures,
                totalOverrides,
                totalExtraCharges,
                unpaidExtraCharges,

                collectedToday = collToday.Sum(f => f.PaidAmount ?? 0),
                collectionsToday = collToday.Count,

                collectedThisMonth = collThisMonth.Sum(f => f.PaidAmount ?? 0),
                totalAmtThisMonth = collThisMonth.Sum(f => f.TotalAmount ?? 0),
                discountThisMonth = collThisMonth.Sum(f => f.DiscountAmount ?? 0),
                fineThisMonth = collThisMonth.Sum(f => f.FineAmount ?? 0),
                collectionsThisMonth = collThisMonth.Count,

                collectedInRange = collFiltered.Sum(f => f.PaidAmount ?? 0),
                totalAmtInRange = collFiltered.Sum(f => f.TotalAmount ?? 0),
                discountInRange = collFiltered.Sum(f => f.DiscountAmount ?? 0),
                fineInRange = collFiltered.Sum(f => f.FineAmount ?? 0),
                collectionsInRange = collFiltered.Count,

                collectedAllTime = collAll.Sum(f => f.PaidAmount ?? 0),
                collectionsAllTime = collAll.Count,

                totalDueAmount = dues.Sum(d => (d.TotalDue ?? 0) - (d.PaidAmount ?? 0)),
                pendingDuesCount = dues.Count(d => d.IsSettled != true),
                settledDuesCount = dues.Count(d => d.IsSettled == true),
                studentsWithDues = dues.Where(d => d.IsSettled != true).Select(d => d.StudentId).Distinct().Count()
            });
        }

        // ── Monthly Chart ─────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetMonthlyChart(int? sessionId, int? year)
        {
            var yr = year ?? DateTime.Now.Year;
            var query = _context.TblFeeCollections.Where(f => f.IsActive == true && f.Year == yr);
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(f => f.SessionId == sid);
            }

            var raw = await query
                .GroupBy(f => f.Month)
                .Select(g => new
                {
                    Month = g.Key ?? 0,
                    Paid = g.Sum(f => f.PaidAmount ?? 0),
                    Total = g.Sum(f => f.TotalAmount ?? 0)
                })
                .ToListAsync();

            var months = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            var paid = new decimal[12];
            var total = new decimal[12];

            foreach (var d in raw)
                if (d.Month >= 1 && d.Month <= 12)
                { paid[d.Month - 1] = d.Paid; total[d.Month - 1] = d.Total; }

            return Json(new { months, paid, total });
        }

        // ── Payment Mode Chart ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetPaymentModeChart(int? sessionId)
        {
            var query = _context.TblFeeCollections.Where(f => f.IsActive == true);
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(f => f.SessionId == sid);
            }

            var data = await query
                .GroupBy(f => f.PaymentMode)
                .Select(g => new
                {
                    mode = g.Key ?? "Unknown",
                    amount = g.Sum(f => f.PaidAmount ?? 0),
                    count = g.Count()
                })
                .OrderByDescending(g => g.amount)
                .ToListAsync();

            return Json(data);
        }

        // ── Fee Type Chart ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetFeeTypeChart(int? sessionId)
        {
            var data = await _context.TblFeeCollectionDetails
                .Where(d => d.IsActive == true && d.FeeType != null)
                .GroupBy(d => new { d.FeeTypeId, d.FeeType!.FeeName })
                .Select(g => new
                {
                    feeName = g.Key.FeeName ?? "Other",
                    amount = g.Sum(d => d.Amount ?? 0)
                })
                .OrderByDescending(g => g.amount)
                .ToListAsync();

            return Json(data);
        }

        // ── Due Status ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetDueStatus(int? sessionId)
        {
            var query = _context.TblStudentDues.Where(d => d.IsActive == true);
            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(d => d.SessionId == sid);
            }

            var pending = await query.CountAsync(d => d.IsSettled != true);
            var settled = await query.CountAsync(d => d.IsSettled == true);

            return Json(new { pending, settled });
        }

        // ── Top Defaulters ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetTopDefaulters(int? sessionId)
        {
            IQueryable<TblStudentDue> query = _context.TblStudentDues
                .Where(d => d.IsActive == true
                         && d.IsSettled != true
                         && (d.TotalDue ?? 0) > (d.PaidAmount ?? 0))
                .Include(d => d.Student);

            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(d => d.SessionId == sid);
            }

            // Pull to memory first — GroupBy on navigation props causes EF issues
            var rawData = await query
                .Where(d => d.Student != null)
                .Select(d => new
                {
                    d.StudentId,
                    StudentName = d.Student!.StudentName ?? "-",
                    RollNo = d.Student!.RollNo ?? "-",
                    Remaining = (d.TotalDue ?? 0) - (d.PaidAmount ?? 0)
                })
                .ToListAsync();

            var result = rawData
                .GroupBy(d => new { d.StudentId, d.StudentName, d.RollNo })
                .Select(g => new
                {
                    studentName = g.Key.StudentName,
                    rollNo = g.Key.RollNo,
                    totalPending = g.Sum(d => d.Remaining),
                    monthsCount = g.Count()
                })
                .OrderByDescending(d => d.totalPending)
                .Take(10)
                .ToList();

            return Json(result);
        }

        // ── Recent Transactions ───────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetRecentTransactions(int? sessionId)
        {
            var query = _context.TblFeeCollections
                .Where(f => f.IsActive == true);

            if (sessionId.HasValue && sessionId.Value > 0)
            {
                int sid = sessionId.Value;
                query = query.Where(f => f.SessionId == sid);
            }

            var data = await query
                .OrderByDescending(f => f.CreatedDate)
                .Take(10)
                .Select(f => new
                {
                    id = f.FeeCollectionId,
                    studentName = f.Student != null ? f.Student.StudentName : "-",
                    rollNo = f.Student != null ? f.Student.RollNo : "-",
                    sessionName = f.Session != null ? f.Session.SessionName : "-",
                    monthYear = f.Month != null && f.Year != null
                                  ? System.Globalization.CultureInfo.CurrentCulture
                                      .DateTimeFormat.GetMonthName(f.Month.Value) + " " + f.Year
                                  : "-",
                    paidAmount = f.PaidAmount ?? 0,
                    dueAmount = (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0),
                    paymentMode = f.PaymentMode ?? "-",
                    paymentDate = f.PaymentDate.HasValue
                                  ? f.PaymentDate.Value.ToString("dd-MM-yyyy") : "-"
                })
                .ToListAsync();

            return Json(data);
        }

        // ── Sessions Dropdown ─────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();

            return Json(sessions);
        }
    }
}