using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Communication/Notification/[action]")]
    public class CommNotificationController : Controller
    {
        private readonly LibmanagementContext _db;

        public CommNotificationController(LibmanagementContext db) => _db = db;

        // ── ALL NOTIFICATIONS PAGE ────────────────────────────────────────
        [HttpGet]
        [Route("/Communication/Notifications")]
        public async Task<IActionResult> All(string? type, int page = 1)
        {
            var myType = GetMyType();
            var myId   = GetMyId();
            const int pageSize = 20;

            var query = _db.CommNotifications
                .Where(n => n.RecipientType == myType && n.RecipientId == myId)
                .AsQueryable();

            if (!string.IsNullOrEmpty(type))
                query = query.Where(n => n.NotificationType == type);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.TotalCount  = total;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)pageSize);
            ViewBag.FilterType  = type;
            return View(items);
        }

        // ── AJAX: unread count (bell badge) ───────────────────────────────
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var myType = GetMyType();
            var myId   = GetMyId();
            var count  = await _db.CommNotifications
                .CountAsync(n => n.RecipientType == myType
                              && n.RecipientId == myId
                              && !n.IsRead);
            return Json(new { count });
        }

        // ── AJAX: last 10 for dropdown ────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var myType = GetMyType();
            var myId   = GetMyId();
            var items  = await _db.CommNotifications
                .Where(n => n.RecipientType == myType && n.RecipientId == myId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.NotificationId,
                    n.Title,
                    n.Body,
                    n.IsRead,
                    n.NotificationType,
                    n.Priority,
                    n.RedirectUrl,
                    TimeAgo = n.CreatedAt
                })
                .ToListAsync();
            return Json(items);
        }

        // ── AJAX: mark single read ────────────────────────────────────────
        [HttpPost("{id}")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var item = await _db.CommNotifications.FindAsync(id);
            if (item == null) return Json(new { success = false });
            item.IsRead = true;
            item.ReadAt = DateTime.Now;
            await _db.SaveChangesAsync();
            return Json(new { success = true });
        }

        // ── AJAX: mark all read ───────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var myType = GetMyType();
            var myId   = GetMyId();
            var unread = await _db.CommNotifications
                .Where(n => n.RecipientType == myType
                         && n.RecipientId == myId
                         && !n.IsRead)
                .ToListAsync();
            foreach (var n in unread) { n.IsRead = true; n.ReadAt = DateTime.Now; }
            await _db.SaveChangesAsync();
            return Json(new { success = true, count = unread.Count });
        }

        private string GetMyType()
        {
            var role = Request.Cookies["roleName"] ?? "";
            return role.ToLower() switch
            {
                "student" => "Student",
                "teacher" => "Teacher",
                _         => "Admin"
            };
        }

        private int GetMyId()
        {
            var v = Request.Cookies["EntityId"];
            return int.TryParse(v, out var id) ? id : 0;
        }
    }
}
