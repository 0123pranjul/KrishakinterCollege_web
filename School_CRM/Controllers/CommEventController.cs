using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Communication/Event/[action]")]
    public class CommEventController : Controller
    {
        private readonly LibmanagementContext _db;

        public CommEventController(LibmanagementContext db) => _db = db;

        // ── CALENDAR VIEW ─────────────────────────────────────────────────
        [HttpGet]
        [Route("/Communication/Calendar")]
        public IActionResult Calendar() => View();

        // ── AJAX: FullCalendar.js JSON feed ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CalendarJson(DateOnly? start, DateOnly? end)
        {
            var query = _db.CommEvents.Where(e => e.IsPublished).AsQueryable();

            if (start.HasValue) query = query.Where(e => e.EndDate >= start);
            if (end.HasValue)   query = query.Where(e => e.StartDate <= end);

            var events = await query.Select(e => new
            {
                id    = e.EventId,
                title = e.EventTitle,
                start = e.StartDate.ToString("yyyy-MM-dd"),
                end   = e.EndDate.AddDays(1).ToString("yyyy-MM-dd"),
                color = e.Color,
                allDay = e.IsFullDay,
                extendedProps = new
                {
                    e.EventType,
                    e.Venue,
                    e.Description,
                    startTime = e.StartTime.HasValue ? e.StartTime.Value.ToString("HH:mm") : null,
                    endTime   = e.EndTime.HasValue   ? e.EndTime.Value.ToString("HH:mm")   : null
                }
            }).ToListAsync();

            return Json(events);
        }

        // ── MANAGE LIST ───────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Manage(string? eventType, int? year)
        {
            var query = _db.CommEvents.AsQueryable();
            if (!string.IsNullOrEmpty(eventType)) query = query.Where(e => e.EventType == eventType);
            if (year.HasValue) query = query.Where(e => e.StartDate.Year == year);

            ViewBag.EventType = eventType;
            ViewBag.Year      = year ?? DateTime.Today.Year;
            return View(await query.OrderByDescending(e => e.StartDate).ToListAsync());
        }

        // ── CREATE ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Classes = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
            return View(new CommEvent
            {
                StartDate  = DateOnly.FromDateTime(DateTime.Today),
                EndDate    = DateOnly.FromDateTime(DateTime.Today),
                IsFullDay  = true,
                IsPublished = true,
                TargetType = "All",
                Color      = "#3498db"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommEvent model)
        {
            if (model.EndDate < model.StartDate)
                ModelState.AddModelError("EndDate", "End date must be on or after start date.");

            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
                return View(model);
            }

            // Auto-assign color by event type
            model.Color = model.EventType switch
            {
                "Holiday"  => "#e74c3c",
                "Exam"     => "#e67e22",
                "PTM"      => "#3498db",
                "Function" => "#9b59b6",
                "Sports"   => "#27ae60",
                "Workshop" => "#1abc9c",
                "Meeting"  => "#34495e",
                _          => "#95a5a6"
            };

            model.CreatedBy = UserId();
            model.CreatedAt = DateTime.Now;
            _db.CommEvents.Add(model);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Event created successfully.";
            return RedirectToAction(nameof(Manage));
        }

        // ── EDIT ──────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.CommEvents.FindAsync(id);
            if (item == null) return NotFound();
            ViewBag.Classes = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
            return View(item);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CommEvent model)
        {
            if (id != model.EventId) return BadRequest();
            if (model.EndDate < model.StartDate)
                ModelState.AddModelError("EndDate", "End date must be on or after start date.");

            if (!ModelState.IsValid)
            {
                ViewBag.Classes = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
                return View(model);
            }

            var existing = await _db.CommEvents.FindAsync(id);
            if (existing == null) return NotFound();

            existing.EventTitle    = model.EventTitle;
            existing.Description   = model.Description;
            existing.EventType     = model.EventType;
            existing.StartDate     = model.StartDate;
            existing.EndDate       = model.EndDate;
            existing.StartTime     = model.StartTime;
            existing.EndTime       = model.EndTime;
            existing.IsFullDay     = model.IsFullDay;
            existing.Venue         = model.Venue;
            existing.TargetType    = model.TargetType;
            existing.TargetClassId = model.TargetClassId;
            existing.Color         = model.Color;
            existing.IsPublished   = model.IsPublished;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Event updated.";
            return RedirectToAction(nameof(Manage));
        }

        // ── DELETE (soft) ─────────────────────────────────────────────────
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.CommEvents.FindAsync(id);
            if (item == null) return NotFound();
            item.IsPublished = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Event removed from calendar.";
            return RedirectToAction(nameof(Manage));
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
