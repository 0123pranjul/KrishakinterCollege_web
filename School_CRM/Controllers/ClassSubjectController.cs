using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class ClassSubjectController : Controller
    {
        private readonly LibmanagementContext _context;
        public ClassSubjectController(LibmanagementContext context) => _context = context;

        public IActionResult Index() => View();

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _context.TblClassSubjects
                .Where(cs => cs.IsActive == true)
                .Include(cs => cs.Class)
                .Include(cs => cs.Subject)
                .Select(cs => new
                {
                    cs.Id,
                    ClassName = cs.Class.ClassName,
                    SubjectName = cs.Subject.SubjectName,
                    Status = cs.IsActive == true ? "Active" : "Inactive",
                    CreatedDate = cs.CreatedDate.HasValue ? cs.CreatedDate.Value.ToString("dd-MM-yyyy") : "-"
                }).ToListAsync();
            return Json(new { data });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            await LoadDropdowns();
            if (id == 0) return PartialView("_ClassSubjectModal", new TblClassSubject { IsActive = true });
            var item = await _context.TblClassSubjects.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_ClassSubjectModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblClassSubject model)
        {
            try
            {
                bool isDuplicate = await _context.TblClassSubjects
                    .AnyAsync(cs => cs.ClassId == model.ClassId && cs.SubjectId == model.SubjectId && cs.IsActive == true && cs.Id != id);
                if (isDuplicate)
                    return Json(new { success = false, message = "This subject is already mapped to the selected class!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblClassSubjects.Add(model);
                }
                else
                {
                    var existing = await _context.TblClassSubjects.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.ClassId = model.ClassId;
                    existing.SubjectId = model.SubjectId;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = id == 0 ? "Mapping added successfully!" : "Mapping updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblClassSubjects.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Mapping deleted successfully!" });
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
        }


       

      
        [HttpGet]
        public async Task<IActionResult> GetGrouped()
        {
            var classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();

            var data = await _context.TblClassSubjects
                .Where(cs => cs.IsActive == true)
                .Include(cs => cs.Class)
                .Include(cs => cs.Subject)
                .Select(cs => new {
                    cs.Id,
                    cs.ClassId,
                    cs.SubjectId,
                    SubjectName = cs.Subject.SubjectName
                }).ToListAsync();

            return Json(new { success = true, data, classes });
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectsForClass(int classId)
        {
            var all = await _context.TblSubjects
                .Where(s => s.IsActive == true)
                .ToListAsync();

            var mapped = await _context.TblClassSubjects
                .Where(cs => cs.ClassId == classId && cs.IsActive == true)
                .Select(cs => cs.SubjectId)
                .ToListAsync();

            return Json(new
            {
                subjects = all.Select(s => new {
                    s.SubjectId,
                    s.SubjectName,
                    IsMapped = mapped.Contains(s.SubjectId)
                })
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkSave(int classId, List<int> subjectIds)
        {
            try
            {
                if (subjectIds == null) subjectIds = new List<int>();

                var existing = await _context.TblClassSubjects
                    .Where(cs => cs.ClassId == classId && cs.IsActive == true)
                    .ToListAsync();

                var existingIds = existing.Select(e => e.SubjectId).ToList();

                var toAdd = subjectIds.Except(existingIds);
                foreach (var sid in toAdd)
                    _context.TblClassSubjects.Add(new TblClassSubject
                    {
                        ClassId = classId,
                        SubjectId = sid,
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    });

                var toRemove = existing.Where(e => !subjectIds.Contains(e.SubjectId));
                foreach (var item in toRemove)
                {
                    item.IsActive = false;
                    item.UpdatedDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Mapping save ho gayi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
