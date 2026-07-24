using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class FeeStatusReportController : Controller
    {
        private readonly LibmanagementContext _context;

        public FeeStatusReportController(LibmanagementContext context)
        {
            _context = context;
        }

        public IActionResult Index() => View();

        // GET: Dropdowns
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

            return Json(new { sessions, classes });
        }

        // GET: Sections by class
        [HttpGet]
        public async Task<IActionResult> GetSections(int classId)
        {
            var sections = await _context.TblSections
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName })
                .ToListAsync();
            return Json(sections);
        }

        // GET: Full Year Report — student x month grid
        [HttpGet]
        public async Task<IActionResult> GetReport(int sessionId, int classId, int? sectionId)
        {
            if (sessionId <= 0 || classId <= 0)
                return Json(new { success = false, message = "Session aur Class select karo!" });

            // Students in this class+session
            var query = _context.TblStudentSessions
                .Where(ss => ss.SessionId == sessionId
                          && ss.ClassId   == classId
                          && ss.IsActive  == true
                          && ss.Student   != null
                          && ss.Student.IsActive == true)
                .Include(ss => ss.Student)
                .Include(ss => ss.Section)
                .AsQueryable();

            if (sectionId.HasValue && sectionId.Value > 0)
                query = query.Where(ss => ss.SectionId == sectionId.Value);

            var students = await query
                .OrderBy(ss => ss.Student!.StudentName)
                .Select(ss => new {
                    StudentId   = ss.Student!.StudentId,
                    StudentName = ss.Student.StudentName ?? "-",
                    RollNo      = ss.Student.RollNo      ?? "-",
                    SectionName = ss.Section != null ? ss.Section.SectionName : "-"
                })
                .ToListAsync();

            if (!students.Any())
                return Json(new { success = false, message = "Is class mein koi student nahi mila!" });

            var studentIds = students.Select(s => s.StudentId).ToList();

            // All collections for these students in this session
            var collections = await _context.TblFeeCollections
                .Where(f => f.SessionId == sessionId
                         && f.IsActive  == true
                         && studentIds.Contains(f.StudentId ?? 0)
                         && f.Month != null && f.Year != null)
                .Select(f => new {
                    f.StudentId,
                    f.Month,
                    f.Year,
                    TotalAmount = f.TotalAmount ?? 0,
                    PaidAmount  = f.PaidAmount  ?? 0,
                    DueAmount   = (f.TotalAmount ?? 0) - (f.PaidAmount ?? 0)
                })
                .ToListAsync();

            // Build lookup: studentId -> { month_year -> status }
            var colLookup = collections
                .GroupBy(c => c.StudentId ?? 0)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToDictionary(
                        c => $"{c.Month}_{c.Year}",
                        c => c.DueAmount <= 0 ? "Paid" : "Partial"
                    )
                );

            // All unique year values in collections
            var years = collections.Select(c => c.Year!.Value).Distinct().OrderBy(y => y).ToList();
            if (!years.Any()) years.Add(DateTime.Now.Year);

            // Build result rows
            var rows = students.Select(s => {
                colLookup.TryGetValue(s.StudentId, out var monthMap);
                return new {
                    s.StudentId,
                    s.StudentName,
                    s.RollNo,
                    s.SectionName,
                    MonthStatus = monthMap ?? new Dictionary<string, string>()
                };
            }).ToList<dynamic>();

            // Deduplicate and sort: Apr-Mar order
            var sortedMonthCols = years
                .SelectMany(yr => new[] {4,5,6,7,8,9,10,11,12,1,2,3}
                    .Select(m => new {
                        month = m,
                        year  = (m >= 4) ? yr : yr + 1,
                        label = System.Globalization.CultureInfo.CurrentCulture
                                    .DateTimeFormat.GetMonthName(m).Substring(0, 3) +
                                " " + ((m >= 4) ? yr : yr + 1)
                    }))
                .DistinctBy(x => $"{x.month}_{x.year}")
                .ToList<dynamic>();

            return Json(new { success = true, students = rows, months = sortedMonthCols });
        }
    }
}
