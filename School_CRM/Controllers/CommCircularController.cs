using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Communication/Circular/[action]")]
    public class CommCircularController : Controller
    {
        private readonly LibmanagementContext _db;
        private readonly IWebHostEnvironment  _env;

        public CommCircularController(LibmanagementContext db, IWebHostEnvironment env)
        {
            _db  = db;
            _env = env;
        }

        [HttpGet]
        [Route("/Communication/Circulars")]
        public async Task<IActionResult> Index(string? targetType, DateOnly? fromDate, DateOnly? toDate)
        {
            var query = _db.CommCirculars.Where(c => c.IsActive).AsQueryable();

            if (!string.IsNullOrEmpty(targetType))
                query = query.Where(c => c.TargetType == targetType);
            if (fromDate.HasValue)
                query = query.Where(c => c.CircularDate >= fromDate);
            if (toDate.HasValue)
                query = query.Where(c => c.CircularDate <= toDate);

            ViewBag.TargetType = targetType;
            ViewBag.FromDate   = fromDate;
            ViewBag.ToDate     = toDate;

            return View(await query.OrderByDescending(c => c.CircularDate).ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new CommCircular
            {
                CircularDate = DateOnly.FromDateTime(DateTime.Today),
                TargetType   = "All",
                IsActive     = true
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CommCircular model, IFormFile pdfFile)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                ModelState.AddModelError("", "PDF file is required.");
                await LoadDropdowns();
                return View(model);
            }
            if (!pdfFile.ContentType.Contains("pdf"))
            {
                ModelState.AddModelError("", "Only PDF files are allowed.");
                await LoadDropdowns();
                return View(model);
            }
            if (pdfFile.Length > 10 * 1024 * 1024)
            {
                ModelState.AddModelError("", "File size cannot exceed 10 MB.");
                await LoadDropdowns();
                return View(model);
            }

            // Auto-generate circular number: CIR-YYYY-NNN
            var year  = DateTime.Today.Year;
            var count = await _db.CommCirculars
                .CountAsync(c => c.CircularNo.StartsWith($"CIR-{year}-")) + 1;
            model.CircularNo = $"CIR-{year}-{count:D3}";

            // Save PDF
            var dir = Path.Combine(_env.WebRootPath, "circulars");
            Directory.CreateDirectory(dir);
            var fileName = $"{model.CircularNo}.pdf";
            using var stream = new FileStream(Path.Combine(dir, fileName), FileMode.Create);
            await pdfFile.CopyToAsync(stream);

            model.FilePath   = $"/circulars/{fileName}";
            model.FileName   = pdfFile.FileName;
            model.FileSizeKb = (int)(pdfFile.Length / 1024);
            model.CreatedBy  = UserId();
            model.CreatedAt  = DateTime.Now;

            _db.CommCirculars.Add(model);
            await _db.SaveChangesAsync();

            // Notify target users
            await SendCircularNotificationsAsync(model);

            TempData["Success"] = $"Circular {model.CircularNo} uploaded successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _db.CommCirculars.FindAsync(id);
            if (item == null) return NotFound();
            item.IsActive = false;
            await _db.SaveChangesAsync();
            TempData["Success"] = "Circular deactivated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Download(int id)
        {
            var item = await _db.CommCirculars.FindAsync(id);
            if (item == null) return NotFound();
            var fullPath = Path.Combine(_env.WebRootPath, item.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(fullPath)) return NotFound("File not found on server.");
            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/pdf", item.FileName);
        }

        private async Task SendCircularNotificationsAsync(CommCircular circular)
        {
            var recipients = new List<(string type, int id)>();

            if (circular.TargetType == "All" || circular.TargetType == "AllStudents")
            {
                var ids = await _db.TblStudents.Where(s => s.IsActive == true).Select(s => s.StudentId).ToListAsync();
                recipients.AddRange(ids.Select(id => ("Student", id)));
            }
            if (circular.TargetType == "All" || circular.TargetType == "AllTeachers")
            {
                var ids = await _db.TblTeachers.Where(t => t.IsActive == true).Select(t => t.TeacherId).ToListAsync();
                recipients.AddRange(ids.Select(id => ("Teacher", id)));
            }

            var notifications = recipients.Select(r => new CommNotification
            {
                RecipientType    = r.type,
                RecipientId      = r.id,
                Title            = $"New Circular: {circular.Title}",
                Body             = $"Circular {circular.CircularNo} has been uploaded. Click to download.",
                NotificationType = "CircularUploaded",
                RedirectUrl      = $"/Communication/Circular/Download/{circular.CircularId}",
                ReferenceId      = circular.CircularId,
                ReferenceType    = "CommCircular",
                Priority         = "Normal",
                IsRead           = false,
                CreatedAt        = DateTime.Now,
                CreatedBy        = circular.CreatedBy
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

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
