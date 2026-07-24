using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ClassSectionController : Controller
    {
        private readonly LibmanagementContext _context;

        public ClassSectionController(
            LibmanagementContext context)
        {
            _context = context;
        }

        private int CurrentUserId =>
            int.TryParse(
                HttpContext.Request.Cookies["EmployeeId"],
                out var id) ? id : 0;

        public IActionResult Index() => View();

        // ── GET ALL MAPPINGS ──────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll(int? sessionId)
        {
            var query = _context.TblClassSections
                .Include(x => x.Class)
                .Include(x => x.Section)
                .Include(x => x.Session)
                .Where(x => x.IsActive == true)
                .AsQueryable();

            if (sessionId.HasValue && sessionId > 0)
                query = query.Where(
                    x => x.SessionId == sessionId);

            var list = await query
                .OrderBy(x => x.Class.ClassName)
                .ThenBy(x => x.Section.SectionName)
                .Select(x => new {
                    x.Id,
                    x.ClassId,
                    className = x.Class.ClassName,
                    x.SectionId,
                    sectionName = x.Section.SectionName,
                    x.SessionId,
                    sessionName = x.Session.SessionName,
                    x.IsActive
                }).ToListAsync();

            return Json(new { data = list });
        }

        // ── GET CLASSES ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var list = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .OrderBy(c => c.ClassName)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();
            return Json(list);
        }

        // ── GET SECTIONS ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSections()
        {
            var list = await _context.TblSections
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.SectionName)
                .Select(s => new { s.SectionId, s.SectionName })
                .ToListAsync();
            return Json(list);
        }

        // ── GET SESSIONS ──────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSessions()
        {
            var list = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .Select(s => new { s.SessionId, s.SessionName })
                .ToListAsync();
            return Json(list);
        }

        // ── GET SECTIONS MAPPED TO CLASS ──────────────────────
        [HttpGet]
        public async Task<IActionResult> GetMappedSections(
            int classId, int sessionId)
        {
            var mapped = await _context.TblClassSections
                .Include(x => x.Section)
                .Where(x => x.ClassId == classId &&
                            x.SessionId == sessionId &&
                            x.IsActive == true)
                .Select(x => new {
                    x.Id,
                    x.SectionId,
                    x.Section.SectionName
                }).ToListAsync();

            return Json(mapped);
        }

        // ── SAVE (Single) ─────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Save(
            [FromBody] ClassSectionDto dto)
        {
            try
            {
                // Duplicate check
                var exists = await _context.TblClassSections
                    .AnyAsync(x =>
                        x.ClassId == dto.ClassId &&
                        x.SectionId == dto.SectionId &&
                        x.SessionId == dto.SessionId &&
                        x.IsActive == true);

                if (exists)
                    return Json(new
                    {
                        success = false,
                        message = "This mapping already exists!"
                    });

                _context.TblClassSections.Add(
                    new TblClassSection
                    {
                        ClassId = dto.ClassId,
                        SectionId = dto.SectionId,
                        SessionId = dto.SessionId,
                        IsActive = true,
                        CreatedBy = CurrentUserId,
                        CreatedDate = DateTime.Now
                    });

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Mapping saved!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── BULK SAVE ─────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> BulkSave(
            [FromBody] BulkMappingDto dto)
        {
            try
            {
                if (dto.SessionId <= 0 || dto.ClassId <= 0)
                    return Json(new
                    {
                        success = false,
                        message = "Select class and session!"
                    });

                // Get existing active mappings
                var existing = await _context.TblClassSections
                    .Where(x =>
                        x.ClassId == dto.ClassId &&
                        x.SessionId == dto.SessionId &&
                        x.IsActive == true)
                    .ToListAsync();

                var existingSectionIds = existing
                    .Select(x => x.SectionId).ToList();

                // Sections to ADD
                var toAdd = dto.SectionIds
                    .Where(s => !existingSectionIds.Contains(s))
                    .ToList();

                // Sections to REMOVE
                var toRemove = existing
                    .Where(x => !dto.SectionIds
                        .Contains(x.SectionId))
                    .ToList();

                // Soft delete removed
                toRemove.ForEach(x => {
                    x.IsActive = false;
                    x.UpdatedBy = CurrentUserId;
                    x.UpdatedDate = DateTime.Now;
                });

                // Add new
                toAdd.ForEach(sId =>
                    _context.TblClassSections.Add(
                        new TblClassSection
                        {
                            ClassId = dto.ClassId,
                            SectionId = sId,
                            SessionId = dto.SessionId,
                            IsActive = true,
                            CreatedBy = CurrentUserId,
                            CreatedDate = DateTime.Now
                        }));

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = $"{toAdd.Count} added, " +
                              $"{toRemove.Count} removed!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── DELETE ────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(
            [FromBody] int id)
        {
            try
            {
                var item = await _context.TblClassSections
                    .FindAsync(id);
                if (item == null)
                    return Json(new
                    {
                        success = false,
                        message = "Not found!"
                    });

                item.IsActive = false;
                item.UpdatedBy = CurrentUserId;
                item.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new
                {
                    success = true,
                    message = "Deleted!"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }

        // ── GET SUMMARY ───────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetSummary(
            int sessionId)
        {
            var data = await _context.TblClassSections
                .Include(x => x.Class)
                .Include(x => x.Section)
                .Where(x => x.SessionId == sessionId &&
                            x.IsActive == true)
                .GroupBy(x => new {
                    x.ClassId,
                    x.Class.ClassName
                })
                .Select(g => new {
                    classId = g.Key.ClassId,
                    className = g.Key.ClassName,
                    sections = g.Select(x => new {
                        x.Id,
                        x.SectionId,
                        x.Section.SectionName
                    }).ToList(),
                    count = g.Count()
                })
                .OrderBy(x => x.className)
                .ToListAsync();

            return Json(new { data });
        }
    }

    // ── DTOs ─────────────────────────────────────────────────
    public class ClassSectionDto
    {
        public int ClassId { get; set; }
        public int SectionId { get; set; }
        public int SessionId { get; set; }
    }

    public class BulkMappingDto
    {
        public int ClassId { get; set; }
        public int SessionId { get; set; }
        public List<int> SectionIds { get; set; } = new();
    }
}