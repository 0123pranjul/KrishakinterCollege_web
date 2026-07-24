using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    public class TimeTableController : Controller
    {
        private readonly LibmanagementContext _context;
        public TimeTableController(LibmanagementContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetGrid(int sessionId, int classId, int sectionId)
        {
            var periods = await _context.TblPeriods
                .Where(p => p.IsActive == true)
                .OrderBy(p => p.SequenceNo)
                .ToListAsync();

            var entries = await _context.TblTimeTables
                .Where(tt => tt.SessionId == sessionId && tt.ClassId == classId && tt.SectionId == sectionId && tt.IsActive == true)
                .Include(tt => tt.Subject)
                .Include(tt => tt.Teacher)
                .Include(tt => tt.Period)
                .ToListAsync();

            var grid = periods.Select(p => new
            {
                p.PeriodId,
                p.PeriodName,
                StartTime = p.StartTime.ToString("hh\\:mm"),
                EndTime = p.EndTime.ToString("hh\\:mm"),
                p.IsBrake,
                Days = Enumerable.Range(1, 6).Select(day =>
                {
                    var entry = entries.FirstOrDefault(e => e.PeriodId == p.PeriodId && e.DayOfWeek == day);
                    return new
                    {
                        Day = day,
                        TimeTableId = entry?.TimeTableId ?? 0,
                        SubjectName = entry?.Subject?.SubjectName ?? "",
                        TeacherName = entry?.Teacher?.TeacherName ?? ""
                    };
                }).ToList()
            }).ToList();

            return Json(new { data = grid });
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0, int sessionId = 0, int classId = 0, int sectionId = 0, int periodId = 0, int dayOfWeek = 0)
        {
            await LoadDropdowns();
            if (id == 0)
            {
                return PartialView("_TimeTableModal", new TblTimeTable
                {
                    SessionId = sessionId, ClassId = classId, SectionId = sectionId,
                    PeriodId = periodId, DayOfWeek = (byte)dayOfWeek, IsActive = true
                });
            }
            var item = await _context.TblTimeTables.FindAsync(id);
            if (item == null) return NotFound();
            return PartialView("_TimeTableModal", item);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblTimeTable model)
        {
            try
            {
                bool isDuplicate = await _context.TblTimeTables
                    .AnyAsync(tt => tt.ClassId == model.ClassId && tt.SectionId == model.SectionId
                        && tt.PeriodId == model.PeriodId && tt.DayOfWeek == model.DayOfWeek
                        && tt.SessionId == model.SessionId && tt.IsActive == true && tt.TimeTableId != id);
                if (isDuplicate)
                    return Json(new { success = false, message = "A timetable entry already exists for this slot!" });

                if (id == 0)
                {
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTimeTables.Add(model);
                }
                else
                {
                    var existing = await _context.TblTimeTables.FindAsync(id);
                    if (existing == null) return Json(new { success = false, message = "Record not found!" });
                    existing.TeacherId = model.TeacherId;
                    existing.SubjectId = model.SubjectId;
                    existing.DayOfWeek = model.DayOfWeek;
                    existing.PeriodId = model.PeriodId;
                    existing.SessionId = model.SessionId;
                    existing.ClassId = model.ClassId;
                    existing.SectionId = model.SectionId;
                    existing.IsActive = model.IsActive;
                    existing.UpdatedDate = DateTime.Now;
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Timetable saved successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TblTimeTables.FindAsync(id);
            if (item == null) return Json(new { success = false, message = "Record not found!" });
            item.IsActive = false;
            item.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Entry deleted successfully!" });
        }

        private async Task LoadDropdowns()
        {
            ViewBag.Teachers = await _context.TblTeachers.Where(t => t.IsActive == true).ToListAsync();
            ViewBag.Classes = await _context.TblClasses.Where(c => c.IsActive == true).ToListAsync();
            ViewBag.Sections = await _context.TblSections.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Subjects = await _context.TblSubjects.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Sessions = await _context.TblAcademicSessions.Where(s => s.IsActive == true).ToListAsync();
            ViewBag.Periods = await _context.TblPeriods.Where(p => p.IsActive == true).OrderBy(p => p.SequenceNo).ToListAsync();
        }
    }
}
