using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Communication/Message/[action]")]
    public class CommMessageController : Controller
    {
        private readonly LibmanagementContext _db;
        private readonly IWebHostEnvironment  _env;

        public CommMessageController(LibmanagementContext db, IWebHostEnvironment env)
        {
            _db  = db;
            _env = env;
        }

        // ── INBOX ─────────────────────────────────────────────────────────
        [HttpGet]
        [Route("/Communication/Messages")]
        public async Task<IActionResult> Inbox()
        {
            var myType = GetMyType();
            var myId   = GetMyId();

            var threads = await _db.CommMessageThreads
                .Where(t => t.IsActive &&
                           ((t.InitiatorType == myType && t.InitiatorId == myId) ||
                            (t.RecipientType == myType && t.RecipientId == myId)))
                .OrderByDescending(t => t.LastMessageAt)
                .ToListAsync();

            // Unread count per thread
            var threadIds = threads.Select(t => t.ThreadId).ToList();
            var unreadCounts = await _db.CommMessages
                .Where(m => threadIds.Contains(m.ThreadId)
                         && m.SenderType != myType
                         && !m.IsRead && !m.IsDeleted)
                .GroupBy(m => m.ThreadId)
                .Select(g => new { ThreadId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ThreadId, x => x.Count);

            ViewBag.UnreadCounts = unreadCounts;
            ViewBag.MyType       = myType;
            ViewBag.MyId         = myId;
            return View(threads);
        }

        // ── THREAD VIEW ───────────────────────────────────────────────────
        [HttpGet("{threadId}")]
        public async Task<IActionResult> Thread(int threadId)
        {
            var myType = GetMyType();
            var myId   = GetMyId();

            var thread = await _db.CommMessageThreads
                .FirstOrDefaultAsync(t => t.ThreadId == threadId && t.IsActive &&
                    ((t.InitiatorType == myType && t.InitiatorId == myId) ||
                     (t.RecipientType == myType && t.RecipientId == myId)));

            if (thread == null) return NotFound();

            var messages = await _db.CommMessages
                .Where(m => m.ThreadId == threadId && !m.IsDeleted)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            // Mark unread messages as read
            var unread = messages.Where(m => m.SenderType != myType && !m.IsRead).ToList();
            foreach (var m in unread) { m.IsRead = true; m.ReadAt = DateTime.Now; }
            if (unread.Any()) await _db.SaveChangesAsync();

            ViewBag.Thread = thread;
            ViewBag.MyType = myType;
            ViewBag.MyId   = myId;
            return View(messages);
        }

        // ── COMPOSE ───────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Compose()
        {
            await LoadRecipientDropdowns();
            return View(new CommMessage());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(string recipientType, int recipientId,
            string? subject, int? studentId, string messageBody, IFormFile? attachment)
        {
            if (string.IsNullOrWhiteSpace(messageBody))
            {
                ModelState.AddModelError("", "Message body is required.");
                await LoadRecipientDropdowns();
                return View(new CommMessage());
            }

            var myType = GetMyType();
            var myId   = GetMyId();

            // Find or create thread
            var thread = await _db.CommMessageThreads.FirstOrDefaultAsync(t =>
                t.IsActive &&
                ((t.InitiatorType == myType && t.InitiatorId == myId &&
                  t.RecipientType == recipientType && t.RecipientId == recipientId) ||
                 (t.InitiatorType == recipientType && t.InitiatorId == recipientId &&
                  t.RecipientType == myType && t.RecipientId == myId)));

            if (thread == null)
            {
                thread = new CommMessageThread
                {
                    InitiatorType = myType,
                    InitiatorId   = myId,
                    RecipientType = recipientType,
                    RecipientId   = recipientId,
                    Subject       = subject,
                    StudentId     = studentId,
                    LastMessageAt = DateTime.Now,
                    IsActive      = true,
                    CreatedAt     = DateTime.Now
                };
                _db.CommMessageThreads.Add(thread);
                await _db.SaveChangesAsync();
            }

            // Handle attachment
            string? attachPath = null;
            if (attachment != null && attachment.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "messages");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await attachment.CopyToAsync(stream);
                attachPath = $"/messages/{fileName}";
            }

            var message = new CommMessage
            {
                ThreadId       = thread.ThreadId,
                SenderType     = myType,
                SenderId       = myId,
                MessageBody    = messageBody,
                AttachmentPath = attachPath,
                IsRead         = false,
                SentAt         = DateTime.Now,
                IsDeleted      = false
            };
            _db.CommMessages.Add(message);

            thread.LastMessageAt = DateTime.Now;

            // Notify recipient
            _db.CommNotifications.Add(new CommNotification
            {
                RecipientType    = recipientType,
                RecipientId      = recipientId,
                Title            = "New Message",
                Body             = messageBody.Length > 80 ? messageBody.Substring(0, 80) + "..." : messageBody,
                NotificationType = "MessageReceived",
                RedirectUrl      = $"/Communication/Message/Thread/{thread.ThreadId}",
                ReferenceId      = thread.ThreadId,
                ReferenceType    = "CommMessageThread",
                Priority         = "Normal",
                IsRead           = false,
                CreatedAt        = DateTime.Now,
                CreatedBy        = myId
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Thread), new { threadId = thread.ThreadId });
        }

        // ── REPLY ─────────────────────────────────────────────────────────
        [HttpPost("{threadId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reply(int threadId, string messageBody, IFormFile? attachment)
        {
            var myType = GetMyType();
            var myId   = GetMyId();

            var thread = await _db.CommMessageThreads.FindAsync(threadId);
            if (thread == null) return NotFound();

            string? attachPath = null;
            if (attachment != null && attachment.Length > 0)
            {
                var dir = Path.Combine(_env.WebRootPath, "messages");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await attachment.CopyToAsync(stream);
                attachPath = $"/messages/{fileName}";
            }

            _db.CommMessages.Add(new CommMessage
            {
                ThreadId       = threadId,
                SenderType     = myType,
                SenderId       = myId,
                MessageBody    = messageBody,
                AttachmentPath = attachPath,
                IsRead         = false,
                SentAt         = DateTime.Now,
                IsDeleted      = false
            });

            thread.LastMessageAt = DateTime.Now;

            // Notify the other party
            var recipientType = thread.InitiatorType == myType ? thread.RecipientType : thread.InitiatorType;
            var recipientId   = thread.InitiatorType == myType ? thread.RecipientId   : thread.InitiatorId;

            _db.CommNotifications.Add(new CommNotification
            {
                RecipientType    = recipientType,
                RecipientId      = recipientId,
                Title            = "New Reply",
                Body             = messageBody.Length > 80 ? messageBody.Substring(0, 80) + "..." : messageBody,
                NotificationType = "MessageReceived",
                RedirectUrl      = $"/Communication/Message/Thread/{threadId}",
                Priority         = "Normal",
                IsRead           = false,
                CreatedAt        = DateTime.Now,
                CreatedBy        = myId
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Thread), new { threadId });
        }

        // ── HELPERS ───────────────────────────────────────────────────────
        private async Task LoadRecipientDropdowns()
        {
            ViewBag.Teachers = await _db.TblTeachers
                .Where(t => t.IsActive == true)
                .Select(t => new { t.TeacherId, t.TeacherName })
                .ToListAsync();
            ViewBag.Students = await _db.TblStudents
                .Where(s => s.IsActive == true)
                .Select(s => new { s.StudentId, s.StudentName })
                .ToListAsync();
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
