using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Communication/Announcement/[action]")]
    public class CommAnnouncementController : Controller
    {
        private readonly LibmanagementContext _db;
        private readonly IWebHostEnvironment  _env;

        public CommAnnouncementController(LibmanagementContext db, IWebHostEnvironment env)
        {
            _db  = db;
            _env = env;
        }

        // ── NOTICE BOARD (all users, role-filtered) ───────────────────────
        [HttpGet]
        [Route("/Communication/Announcements")]
        public async Task<IActionResult> Index()
        {
            var role      = Request.Cookies["roleName"] ?? "";
            var classId   = int.TryParse(Request.Cookies["ClassId"],   out var cid) ? cid : 0;
            var sectionId = int.TryParse(Request.Cookies["SectionId"], out var sid) ? sid : 0;
            var now       = DateTime.Now;

            var query = _db.CommAnnouncements
                .Where(a => a.IsPublished
                         && a.PublishAt <= now
                         && (a.ExpiresAt == null || a.ExpiresAt >= now));

            // Role-based filter
            if (!IsAdmin(role))
            {
                query = query.Where(a =>
                    a.TargetType == "All" ||
                    (a.TargetType == "AllStudents" && role.ToLower() == "student") ||
                    (a.TargetType == "AllTeachers" && role.ToLower() == "teacher") ||
                    (a.TargetType == "ClassWise"   && a.TargetClassId == classId) ||
                    (a.TargetType == "SectionWise" && a.TargetSectionId == sectionId));
            }

            var announcements = await query
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishAt)
                .ToListAsync();

            ViewBag.IsAdmin = IsAdmin(role);
            return View(announcements);
        }

        // ── ADMIN LIST ────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var list = await _db.CommAnnouncements
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            return View(list);
        }

        // ── CREATE ────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new CommAnnouncement
            {
                TargetType  = "All",
                Priority    = "Normal",
                PublishAt   = DateTime.Now,
                IsPublished = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommAnnouncement model, IFormFile? attachment)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(); return View(model); }

            // Handle PDF attachment
            if (attachment != null && attachment.Length > 0)
            {
                if (!attachment.ContentType.Contains("pdf"))
                {
                    ModelState.AddModelError("", "Only PDF files are allowed.");
                    await LoadDropdowns();
                    return View(model);
                }
                if (attachment.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File size cannot exceed 5 MB.");
                    await LoadDropdowns();
                    return View(model);
                }
                var dir = Path.Combine(_env.WebRootPath, "announcements");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}.pdf";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await attachment.CopyToAsync(stream);
                model.AttachmentPath = $"/announcements/{fileName}";
                model.AttachmentName = attachment.FileName;
            }

            model.CreatedBy = UserId();
            model.CreatedAt = DateTime.Now;
            _db.CommAnnouncements.Add(model);
            await _db.SaveChangesAsync();

            // Send in-app notifications if published now
            if (model.IsPublished && model.PublishAt <= DateTime.Now)
                await SendAnnouncementNotificationsAsync(model);

            TempData["Success"] = "Announcement created successfully.";
            return RedirectToAction(nameof(Manage));
        }

        // ── EDIT ──────────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _db.CommAnnouncements.FindAsync(id);
            if (item == null) return NotFound();
            await LoadDropdowns();
            return View(item);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CommAnnouncement model, IFormFile? attachment)
        {
            if (id != model.AnnouncementId) return BadRequest();
            var existing = await _db.CommAnnouncements.FindAsync(id);
            if (existing == null) return NotFound();

            if (attachment != null && attachment.Length > 0)
            {
                if (!attachment.ContentType.Contains("pdf"))
                { ModelState.AddModelError("", "Only PDF files are allowed."); await LoadDropdowns(); return View(model); }

                var dir = Path.Combine(_env.WebRootPath, "announcements");
                Directory.CreateDirectory(dir);
                var fileName = $"{Guid.NewGuid()}.pdf";
                using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
                await attachment.CopyToAsync(stream);
                existing.AttachmentPath = $"/announcements/{fileName}";
                existing.AttachmentName = attachment.FileName;
            }

            existing.Title          = model.Title;
            existing.Body           = model.Body;
            existing.TargetType     = model.TargetType;
            existing.TargetClassId  = model.TargetClassId;
            existing.TargetSectionId = model.TargetSectionId;
            existing.Priority       = model.Priority;
            existing.PublishAt      = model.PublishAt;
            existing.ExpiresAt      = model.ExpiresAt;
            existing.IsPublished    = model.IsPublished;
            existing.IsPinned       = model.IsPinned;
            existing.UpdatedAt      = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Announcement updated.";
            return RedirectToAction(nameof(Manage));
        }

        // ── VIEW DETAIL + READ TRACKING ───────────────────────────────────
        [HttpGet("{id}")]
        public async Task<IActionResult> View(int id)
        {
            var item = await _db.CommAnnouncements
                .Include(a => a.CommAnnouncementReads)
                .FirstOrDefaultAsync(a => a.AnnouncementId == id);
            if (item == null) return NotFound();

            // Track read
            var readerType = GetReaderType();
            var readerId   = GetEntityId();
            bool alreadyRead = await _db.CommAnnouncementReads
                .AnyAsync(r => r.AnnouncementId == id
                            && r.ReaderType == readerType
                            && r.ReaderId == readerId);
            if (!alreadyRead && readerId > 0)
            {
                _db.CommAnnouncementReads.Add(new CommAnnouncementRead
                {
                    AnnouncementId = id,
                    ReaderType     = readerType,
                    ReaderId       = readerId,
                    ReadAt         = DateTime.Now
                });
                await _db.SaveChangesAsync();
            }

            ViewBag.ReadCount = item.CommAnnouncementReads.Count;
            return View(item);
        }

        // ── DELETE ────────────────────────────────────────────────────────
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.CommAnnouncements.FindAsync(id);
            if (item == null) return Json(new { success = false });
            _db.CommAnnouncements.Remove(item);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Announcement deleted.";
            return RedirectToAction(nameof(Manage));
        }

        // ── HELPERS ───────────────────────────────────────────────────────
        private async Task SendAnnouncementNotificationsAsync(CommAnnouncement ann)
        {
            var recipients = new List<(string type, int id)>();

            if (ann.TargetType == "All" || ann.TargetType == "AllStudents")
            {
                var students = await _db.TblStudents
                    .Where(s => s.IsActive == true)
                    .Select(s => s.StudentId).ToListAsync();
                recipients.AddRange(students.Select(id => ("Student", id)));
            }
            if (ann.TargetType == "All" || ann.TargetType == "AllTeachers")
            {
                var teachers = await _db.TblTeachers
                    .Where(t => t.IsActive == true)
                    .Select(t => t.TeacherId).ToListAsync();
                recipients.AddRange(teachers.Select(id => ("Teacher", id)));
            }
            if (ann.TargetType == "ClassWise" && ann.TargetClassId.HasValue)
            {
                var students = await _db.TblStudentSessions
                    .Where(ss => ss.ClassId == ann.TargetClassId && ss.IsActive == true)
                    .Select(ss => ss.StudentId)
                    .ToListAsync();
                recipients.AddRange(students
                    .Where(id => id.HasValue)
                    .Select(id => ("Student", id!.Value)));
            }

            var notifications = recipients.Select(r => new CommNotification
            {
                RecipientType    = r.type,
                RecipientId      = r.id,
                Title            = $"New Announcement: {ann.Title}",
                Body             = ann.Body.Length > 100 ? ann.Body.Substring(0, 100) + "..." : ann.Body,
                NotificationType = "Announcement",
                RedirectUrl      = $"/Communication/Announcement/View/{ann.AnnouncementId}",
                ReferenceId      = ann.AnnouncementId,
                ReferenceType    = "CommAnnouncement",
                Priority         = ann.Priority,
                IsRead           = false,
                CreatedAt        = DateTime.Now,
                CreatedBy        = ann.CreatedBy
            }).ToList();

            if (notifications.Any())
            {
                await _db.CommNotifications.AddRangeAsync(notifications);
                await _db.SaveChangesAsync();
            }
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Classes  = new SelectList(await _db.TblClasses.Where(c => c.IsActive == true).ToListAsync(), "ClassId", "ClassName");
            ViewBag.Sections = new SelectList(await _db.TblSections.Where(s => s.IsActive == true).ToListAsync(), "SectionId", "SectionName");
        }

        private bool IsAdmin(string role) =>
            role.ToLower() is "superadmin" or "admin" or "principal";

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }

        private string GetReaderType()
        {
            var role = Request.Cookies["roleName"] ?? "";
            return role.ToLower() switch
            {
                "student" => "Student",
                "teacher" => "Teacher",
                _         => "Admin"
            };
        }

        private int GetEntityId()
        {
            var v = Request.Cookies["EntityId"];
            return int.TryParse(v, out var id) ? id : 0;
        }
    }
}
