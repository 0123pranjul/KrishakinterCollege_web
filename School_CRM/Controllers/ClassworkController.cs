using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace School_CRM.Controllers
{
    public class ClassworkController : Controller
    {
        private readonly LibmanagementContext _context;

        public ClassworkController(LibmanagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetLogs(string startDate, string endDate)
        {
            DateOnly start = string.IsNullOrEmpty(startDate) ? DateOnly.FromDateTime(DateTime.Today.AddDays(-7)) : DateOnly.Parse(startDate);
            DateOnly end = string.IsNullOrEmpty(endDate) ? DateOnly.FromDateTime(DateTime.Today) : DateOnly.Parse(endDate);

            var logsRaw = await _context.TblClassworkLogs
                .Include(l => l.Class)
                .Include(l => l.Section)
                .Include(l => l.Subject)
                .Include(l => l.Employee)
                .Where(l => l.LogDate >= start && l.LogDate <= end)
                .OrderByDescending(l => l.LogDate)
                .ToListAsync();

            var logs = logsRaw.Select(l => new
            {
                id = l.Id,
                logDate = l.LogDate.ToString("yyyy-MM-dd"),
                className = l.Class?.ClassName,
                sectionName = l.Section?.SectionName,
                subjectName = l.Subject?.SubjectName,
                teacherName = l.Employee?.Name,
                topicCovered = l.TopicCovered,
                remarks = l.Remarks
            }).ToList();

            return Json(new { data = logs });
        }

        [HttpGet]
        public async Task<IActionResult> GetFormData()
        {
            var classes = await _context.TblClasses.Where(c => c.IsActive == true).Select(c => new { id = c.ClassId, name = c.ClassName }).ToListAsync();
            var subjects = await _context.TblSubjects.Where(s => s.IsActive == true).Select(s => new { id = s.SubjectId, name = s.SubjectName }).ToListAsync();
            var teachers = await _context.Employees.Select(e => new { id = e.Id, name = e.Name }).ToListAsync();

            return Json(new { classes, subjects, teachers });
        }

        [HttpGet]
        public async Task<IActionResult> GetSectionsByClass(int classId)
        {
            var sections = await _context.TblClassSections
                .Include(cs => cs.Section)
                .Where(cs => cs.ClassId == classId && cs.Section.IsActive == true)
                .Select(cs => new { id = cs.SectionId, name = cs.Section.SectionName })
                .ToListAsync();
            return Json(sections);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLog(int classId, int sectionId, int subjectId, int employeeId, string logDate, string topicCovered, string remarks)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                int userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : 1;

                var log = new TblClassworkLog
                {
                    ClassId = classId,
                    SectionId = sectionId,
                    SubjectId = subjectId,
                    EmployeeId = employeeId,
                    LogDate = DateOnly.Parse(logDate),
                    TopicCovered = topicCovered,
                    Remarks = remarks,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.TblClassworkLogs.Add(log);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
