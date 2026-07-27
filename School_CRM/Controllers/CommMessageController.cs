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

        // Allowed MIME types for attachments
        private static readonly HashSet<string> AllowedMime = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg","image/png","image/gif","image/webp",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        private const long MaxFileBytes = 5 * 1024 * 1024; // 5 MB

        public CommMessageController(LibmanagementContext db, IWebHostEnvironment env)
        {
            _db  = db;
            _env = env;
        }

        // ── HELPERS ───────────────────────────────────────────────────────

        private string GetMyType()
        {
            var role = (Request.Cookies["roleName"] ?? "").ToLower();
            return role switch
            {
                "student"               => "Student",
                "teacher" or "principal"=> "Teacher",
                _                       => "Admin"   // admin, employee, superadmin
            };
        }

        private int GetMyId()
        {
            var v = Request.Cookies["EntityId"];
            return int.TryParse(v, out var id) ? id : 0;
        }

        private string GetMyName()
            => Request.Cookies["EntityName"] ?? "User";

        // Resolve display name for any type+id (same logic as AccountController cookie setting)
        private async Task<string> ResolveNameAsync(string type, int entityId)
        {
            // entityId = what's stored in EntityId cookie (TeacherId / EmpId / UserId)
            UserMaster? user = type switch
            {
                "Teacher" => await _db.UserMasters
                                .Include(u => u.Teacher)
                                .Include(u => u.Emp)
                                .Where(u => u.IsActive == true &&
                                           (u.TeacherId == entityId || u.EmpId == entityId))
                                .FirstOrDefaultAsync(),

                "Student" => await _db.UserMasters
                                .Where(u => u.IsActive == true && u.StudentId == entityId)
                                .FirstOrDefaultAsync(),

                _ => await _db.UserMasters
                                .Include(u => u.Emp)
                                .Where(u => u.IsActive == true && u.EmpId == entityId)
                                .FirstOrDefaultAsync()
            };

            if (user == null) return $"{type} #{entityId}";

            return type switch
            {
                "Teacher" => user.Teacher?.TeacherName
                             ?? user.Emp?.Name
                             ?? user.Username,
                "Student" => user.Username,
                _         => user.Emp?.Name ?? user.Username
            };
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

            var threadIds = threads.Select(t => t.ThreadId).ToList();

            // Unread count per thread (messages sent by the other party)
            var unreadCounts = await _db.CommMessages
                .Where(m => threadIds.Contains(m.ThreadId)
                         && !(m.SenderType == myType && m.SenderId == myId)
                         && !m.IsRead && !m.IsDeleted)
                .GroupBy(m => m.ThreadId)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            // Resolve display names for "other party"
            var otherNames = new Dictionary<int, string>();
            foreach (var t in threads)
            {
                var otherType = (t.InitiatorType == myType && t.InitiatorId == myId)
                    ? t.RecipientType : t.InitiatorType;
                var otherId = (t.InitiatorType == myType && t.InitiatorId == myId)
                    ? t.RecipientId : t.InitiatorId;
                otherNames[t.ThreadId] = await ResolveNameAsync(otherType, otherId);
            }

            ViewBag.UnreadCounts = unreadCounts;
            ViewBag.OtherNames   = otherNames;
            ViewBag.MyType       = myType;
            ViewBag.MyId         = myId;
            ViewBag.MyName       = GetMyName();
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

            // Mark unread as read
            var unread = messages
                .Where(m => !(m.SenderType == myType && m.SenderId == myId) && !m.IsRead)
                .ToList();
            foreach (var m in unread) { m.IsRead = true; m.ReadAt = DateTime.Now; }
            if (unread.Any()) await _db.SaveChangesAsync();

            // Resolve other party name
            var otherType = (thread.InitiatorType == myType && thread.InitiatorId == myId)
                ? thread.RecipientType : thread.InitiatorType;
            var otherId = (thread.InitiatorType == myType && thread.InitiatorId == myId)
                ? thread.RecipientId : thread.InitiatorId;

            ViewBag.Thread      = thread;
            ViewBag.MyType      = myType;
            ViewBag.MyId        = myId;
            ViewBag.MyName      = GetMyName();
            ViewBag.OtherName   = await ResolveNameAsync(otherType, otherId);
            ViewBag.OtherType   = otherType;
            return View(messages);
        }

        // ── POLL: new messages since last id (for auto-refresh) ──────────
        [HttpGet("{threadId}")]
        public async Task<IActionResult> Poll(int threadId, int lastId = 0)
        {
            var myType = GetMyType();
            var myId   = GetMyId();

            // Verify access
            var hasAccess = await _db.CommMessageThreads.AnyAsync(t =>
                t.ThreadId == threadId && t.IsActive &&
                ((t.InitiatorType == myType && t.InitiatorId == myId) ||
                 (t.RecipientType == myType && t.RecipientId == myId)));

            if (!hasAccess) return Forbid();

            var newMsgs = await _db.CommMessages
                .Where(m => m.ThreadId == threadId
                         && m.MessageId > lastId
                         && !m.IsDeleted)
                .OrderBy(m => m.MessageId)
                .ToListAsync();

            // Mark as read
            var unread = newMsgs
                .Where(m => !(m.SenderType == myType && m.SenderId == myId) && !m.IsRead)
                .ToList();
            foreach (var m in unread) { m.IsRead = true; m.ReadAt = DateTime.Now; }
            if (unread.Any()) await _db.SaveChangesAsync();

            return Json(newMsgs.Select(m => new
            {
                m.MessageId,
                m.SenderType,
                m.SenderId,
                isMine = m.SenderType == myType && m.SenderId == myId,
                m.MessageBody,
                m.AttachmentPath,
                attachType = GetAttachType(m.AttachmentPath),
                m.IsRead,
                sentAt = m.SentAt.ToString("hh:mm tt"),
                sentDate = m.SentAt.ToString("dd MMM yyyy")
            }));
        }

        // ── SEND (AJAX) ───────────────────────────────────────────────────
        [HttpPost("{threadId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int threadId, string messageBody, IFormFile? attachment)
        {
            var myType = GetMyType();
            var myId   = GetMyId();

            if (string.IsNullOrWhiteSpace(messageBody) && attachment == null)
                return Json(new { success = false, message = "Message cannot be empty!" });

            var thread = await _db.CommMessageThreads
                .FirstOrDefaultAsync(t => t.ThreadId == threadId && t.IsActive &&
                    ((t.InitiatorType == myType && t.InitiatorId == myId) ||
                     (t.RecipientType == myType && t.RecipientId == myId)));

            if (thread == null)
                return Json(new { success = false, message = "Thread not found!" });

            // Handle attachment
            string? attachPath = null;
            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > MaxFileBytes)
                    return Json(new { success = false, message = "File too large! Max 5 MB." });

                if (!AllowedMime.Contains(attachment.ContentType))
                    return Json(new { success = false, message = "File type not allowed." });

                var dir = Path.Combine(_env.WebRootPath, "messages");
                Directory.CreateDirectory(dir);
                var ext      = Path.GetExtension(attachment.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(dir, fileName);
                using var stream = new FileStream(filePath, FileMode.Create);
                await attachment.CopyToAsync(stream);
                attachPath = $"/messages/{fileName}";
            }

            var msg = new CommMessage
            {
                ThreadId       = threadId,
                SenderType     = myType,
                SenderId       = myId,
                MessageBody    = messageBody?.Trim() ?? "",
                AttachmentPath = attachPath,
                IsRead         = false,
                SentAt         = DateTime.Now,
                IsDeleted      = false
            };
            _db.CommMessages.Add(msg);
            thread.LastMessageAt = DateTime.Now;

            // Notify recipient
            var recipientType = thread.InitiatorType == myType && thread.InitiatorId == myId
                ? thread.RecipientType : thread.InitiatorType;
            var recipientId = thread.InitiatorType == myType && thread.InitiatorId == myId
                ? thread.RecipientId : thread.InitiatorId;

            var preview = string.IsNullOrWhiteSpace(messageBody)
                ? "📎 Attachment"
                : (messageBody.Length > 80 ? messageBody[..80] + "…" : messageBody);

            _db.CommNotifications.Add(new CommNotification
            {
                RecipientType    = recipientType,
                RecipientId      = recipientId,
                Title            = $"New message from {GetMyName()}",
                Body             = preview,
                NotificationType = "MessageReceived",
                RedirectUrl      = $"/Communication/Message/Thread/{threadId}",
                ReferenceId      = threadId,
                ReferenceType    = "CommMessageThread",
                Priority         = "Normal",
                IsRead           = false,
                CreatedAt        = DateTime.Now,
                CreatedBy        = myId
            });

            await _db.SaveChangesAsync();

            return Json(new
            {
                success    = true,
                messageId  = msg.MessageId,
                sentAt     = msg.SentAt.ToString("hh:mm tt"),
                sentDate   = msg.SentAt.ToString("dd MMM yyyy"),
                attachPath,
                attachType = GetAttachType(attachPath)
            });
        }

        // ── COMPOSE ───────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Compose()
        {
            ViewBag.MyType = GetMyType();
            ViewBag.MyName = GetMyName();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Compose(
            string recipientType, int recipientId,
            string? subject, string messageBody, IFormFile? attachment)
        {
            if (string.IsNullOrWhiteSpace(messageBody) && attachment == null)
            {
                ViewBag.Error  = "Message body is required.";
                ViewBag.MyType = GetMyType();
                ViewBag.MyName = GetMyName();
                return View();
            }

            var myType = GetMyType();
            var myId   = GetMyId();

            // Find or create thread
            var thread = await _db.CommMessageThreads.FirstOrDefaultAsync(t =>
                t.IsActive &&
                ((t.InitiatorType == myType  && t.InitiatorId == myId &&
                  t.RecipientType == recipientType && t.RecipientId == recipientId) ||
                 (t.InitiatorType == recipientType && t.InitiatorId == recipientId &&
                  t.RecipientType == myType  && t.RecipientId == myId)));

            if (thread == null)
            {
                thread = new CommMessageThread
                {
                    InitiatorType = myType,
                    InitiatorId   = myId,
                    RecipientType = recipientType,
                    RecipientId   = recipientId,
                    Subject       = subject?.Trim(),
                    LastMessageAt = DateTime.Now,
                    IsActive      = true,
                    CreatedAt     = DateTime.Now
                };
                _db.CommMessageThreads.Add(thread);
                await _db.SaveChangesAsync();
            }

            // Attachment
            string? attachPath = null;
            if (attachment != null && attachment.Length > 0)
            {
                if (attachment.Length > MaxFileBytes)
                {
                    ViewBag.Error  = "File too large! Max 5 MB.";
                    ViewBag.MyType = myType;
                    ViewBag.MyName = GetMyName();
                    return View();
                }

                var dir = Path.Combine(_env.WebRootPath, "messages");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(attachment.FileName)}";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await attachment.CopyToAsync(stream);
                attachPath = $"/messages/{fileName}";
            }

            var msg = new CommMessage
            {
                ThreadId       = thread.ThreadId,
                SenderType     = myType,
                SenderId       = myId,
                MessageBody    = messageBody?.Trim() ?? "",
                AttachmentPath = attachPath,
                IsRead         = false,
                SentAt         = DateTime.Now,
                IsDeleted      = false
            };
            _db.CommMessages.Add(msg);
            thread.LastMessageAt = DateTime.Now;

            var preview = string.IsNullOrWhiteSpace(messageBody)
                ? "📎 Attachment"
                : (messageBody.Length > 80 ? messageBody[..80] + "…" : messageBody);

            _db.CommNotifications.Add(new CommNotification
            {
                RecipientType    = recipientType,
                RecipientId      = recipientId,
                Title            = $"New message from {GetMyName()}",
                Body             = preview,
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

        // ── GET RECIPIENTS (AJAX for Compose dropdown) ───────────────────
        // Uses UserMaster + UserRoleAssign — no direct TblTeacher query
        [HttpGet]
        public async Task<IActionResult> GetRecipients(string type, string? search)
        {
            var q = (search ?? "").Trim().ToLower();

            // Map UI type → RoleName(s) in RoleMaster
            var roleNames = type switch
            {
                "Teacher" => new[] { "teacher", "principal" },
                "Student" => new[] { "student" },
                _         => new[] { "admin", "superadmin", "employee" }
            };

            // Get UserIds that have matching role
            var userIds = await _db.UserRoleAssigns
                .Where(r => r.IsActive == true &&
                            roleNames.Contains(r.Role.RoleName.ToLower()))
                .Select(r => r.UserId)
                .Distinct()
                .ToListAsync();

            if (!userIds.Any())
                return Json(new List<object>());

            // Fetch UserMaster with linked entity name
            var users = await _db.UserMasters
                .Where(u => userIds.Contains(u.UserId) && u.IsActive == true)
                .Select(u => new
                {
                    u.UserId,
                    u.Username,
                    // For display: prefer linked entity name
                    EmpName     = u.Emp != null     ? u.Emp.Name             : null,
                    TeacherName = u.Teacher != null ? u.Teacher.TeacherName  : null,
                    EmpCode     = u.Emp != null     ? u.Emp.EmployeeCode     : null,
                    u.EmpId,
                    u.TeacherId,
                    u.StudentId
                })
                .ToListAsync();

            // Build result — id = EntityId (same as what cookie stores)
            var result = users
                .Where(u =>
                {
                    var displayName = (u.TeacherName ?? u.EmpName ?? u.Username ?? "").ToLower();
                    return q == "" || displayName.Contains(q);
                })
                .Select(u =>
                {
                    // Resolve entityId the same way AccountController does
                    int entityId;
                    string displayName;
                    string code;

                    if (type == "Teacher")
                    {
                        entityId    = u.TeacherId ?? u.EmpId ?? u.UserId;
                        displayName = u.TeacherName ?? u.EmpName ?? u.Username ?? "";
                        code        = u.EmpCode ?? "";
                    }
                    else if (type == "Student")
                    {
                        entityId    = u.StudentId ?? u.UserId;
                        displayName = u.Username ?? "";
                        code        = "";
                    }
                    else
                    {
                        entityId    = u.EmpId ?? u.UserId;
                        displayName = u.EmpName ?? u.Username ?? "";
                        code        = u.EmpCode ?? "";
                    }

                    return new { id = entityId, name = displayName, code };
                })
                .OrderBy(x => x.name)
                .Take(30)
                .ToList();

            return Json(result);
        }

        // ── HELPER: detect attachment type for UI ─────────────────────────
        private static string GetAttachType(string? path)
        {
            if (string.IsNullOrEmpty(path)) return "none";
            var ext = Path.GetExtension(path).ToLower();
            if (ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp") return "image";
            if (ext == ".pdf") return "pdf";
            return "document";
        }
    }
}
