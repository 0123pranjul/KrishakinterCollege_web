using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class HolidayController : Controller
    {
        private readonly LibmanagementContext _context;

        public HolidayController(LibmanagementContext context)
        {
            _context = context;
        }

        private bool IsAdmin =>
            HttpContext.Request.Cookies["IsAdmin"] == "true";

        public IActionResult Index()
        {
            ViewBag.IsAdmin = IsAdmin;
            return View();
        }

        // ── GET ALL ──────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(string? monthYear)
        {
            var query = _context.Holidays.AsQueryable();

            if (!string.IsNullOrEmpty(monthYear))
                query = query.Where(h => h.MonthYear == monthYear);

            var list = await query
                .OrderBy(h => h.HolidayDate)
                .Select(h => new {
                    h.Id,
                    holidayDate = h.HolidayDate.ToString(),
                    h.HolidayName,
                    h.MonthYear,
                    dayName = h.HolidayDate.HasValue
                        ? h.HolidayDate.Value.ToDateTime(TimeOnly.MinValue)
                                              .ToString("dddd")
                        : ""
                }).ToListAsync();

            return Json(new { data = list });
        }

        // ── GET BY ID ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            if (id == 0)
                return Json(new
                {
                    id = 0,
                    holidayDate = "",
                    holidayName = "",
                    monthYear = DateTime.Today.ToString("yyyy-MM")
                });

            var h = await _context.Holidays.FindAsync(id);
            if (h == null) return NotFound();

            return Json(new
            {
                h.Id,
                holidayDate = h.HolidayDate.ToString(),
                h.HolidayName,
                h.MonthYear
            });
        }

        // ── SAVE ─────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Save([FromBody] HolidayDto dto)
        {
            try
            {
                if (!IsAdmin)
                    return Json(new { success = false, message = "Unauthorized!" });

                if (!DateOnly.TryParse(dto.HolidayDate, out var date))
                    return Json(new { success = false, message = "Invalid date!" });

                if (string.IsNullOrEmpty(dto.HolidayName))
                    return Json(new { success = false, message = "Holiday name required!" });

                var monthYear = date.ToString("yyyy-MM");

                if (dto.Id == 0)
                {
                    // Duplicate check
                    var exists = await _context.Holidays
                        .AnyAsync(h => h.HolidayDate == date);

                    if (exists)
                        return Json(new
                        {
                            success = false,
                            message = "Holiday already exists for this date!"
                        });

                    _context.Holidays.Add(new Holiday
                    {
                        HolidayDate = date,
                        HolidayName = dto.HolidayName,
                        MonthYear = monthYear
                    });
                }
                else
                {
                    var existing = await _context.Holidays.FindAsync(dto.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Not found!" });

                    existing.HolidayDate = date;
                    existing.HolidayName = dto.HolidayName;
                    existing.MonthYear = monthYear;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Holiday saved!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── DELETE ───────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete([FromBody] int id)
        {
            try
            {
                if (!IsAdmin)
                    return Json(new { success = false, message = "Unauthorized!" });

                var h = await _context.Holidays.FindAsync(id);
                if (h == null)
                    return Json(new { success = false, message = "Not found!" });

                _context.Holidays.Remove(h);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── BULK ADD (Common holidays) ────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> BulkAdd([FromBody] BulkHolidayDto dto)
        {
            try
            {
                if (!IsAdmin)
                    return Json(new { success = false, message = "Unauthorized!" });

                int added = 0;
                foreach (var item in dto.Holidays)
                {
                    if (!DateOnly.TryParse(item.HolidayDate, out var date)) continue;

                    var exists = await _context.Holidays
                        .AnyAsync(h => h.HolidayDate == date);
                    if (exists) continue;

                    _context.Holidays.Add(new Holiday
                    {
                        HolidayDate = date,
                        HolidayName = item.HolidayName,
                        MonthYear = date.ToString("yyyy-MM")
                    });
                    added++;
                }

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"{added} holidays added!"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── GET MONTH SUMMARY ─────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetMonthSummary(int year)
        {
            var holidays = await _context.Holidays
                .Where(h => h.MonthYear != null &&
                            h.MonthYear.StartsWith(year.ToString()))
                .ToListAsync();

            var result = Enumerable.Range(1, 12).Select(m => new {
                month = new DateTime(year, m, 1).ToString("MMM"),
                monthYear = $"{year}-{m:D2}",
                count = holidays.Count(h => h.MonthYear == $"{year}-{m:D2}")
            }).ToList();

            return Json(new { data = result });
        }
    }

    public class HolidayDto
    {
        public int Id { get; set; }
        public string? HolidayDate { get; set; }
        public string? HolidayName { get; set; }
    }

    public class BulkHolidayDto
    {
        public List<HolidayDto> Holidays { get; set; } = new();
    }
}