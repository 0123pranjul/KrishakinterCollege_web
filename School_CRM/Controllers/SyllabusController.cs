using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace School_CRM.Controllers
{
    public class SyllabusController : Controller
    {
        private readonly LibmanagementContext _context;

        public SyllabusController(LibmanagementContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.TblClasses
                .Where(c => c.IsActive == true)
                .Select(c => new { id = c.ClassId, name = c.ClassName })
                .ToListAsync();
            return Json(classes);
        }

        [HttpGet]
        public async Task<IActionResult> GetSubjectsByClass(int classId)
        {
            var subjects = await _context.TblClassSubjects
                .Include(cs => cs.Subject)
                .Where(cs => cs.ClassId == classId && cs.Subject.IsActive == true)
                .Select(cs => new { id = cs.SubjectId, name = cs.Subject.SubjectName })
                .ToListAsync();
            return Json(subjects);
        }

        [HttpGet]
        public async Task<IActionResult> GetSyllabus(int classId, int subjectId)
        {
            var syllabus = await _context.TblSyllabusUnits
                .Include(u => u.TblSyllabusTopics.Where(t => t.IsActive))
                .Where(u => u.ClassId == classId && u.SubjectId == subjectId && u.IsActive)
                .Select(u => new
                {
                    id = u.Id,
                    unitName = u.UnitName,
                    description = u.Description,
                    topics = u.TblSyllabusTopics.Select(t => new
                    {
                        id = t.Id,
                        topicName = t.TopicName,
                        expectedPeriods = t.ExpectedPeriods
                    }).ToList()
                })
                .ToListAsync();

            return Json(new { success = true, data = syllabus });
        }

        [HttpPost]
        public async Task<IActionResult> SaveUnit(int classId, int subjectId, int unitId, string unitName, string description)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                int userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : 1;

                if (unitId == 0)
                {
                    var unit = new TblSyllabusUnit
                    {
                        ClassId = classId,
                        SubjectId = subjectId,
                        UnitName = unitName,
                        Description = description,
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };
                    _context.TblSyllabusUnits.Add(unit);
                }
                else
                {
                    var unit = await _context.TblSyllabusUnits.FindAsync(unitId);
                    if (unit != null)
                    {
                        unit.UnitName = unitName;
                        unit.Description = description;
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUnit(int id)
        {
            try
            {
                var unit = await _context.TblSyllabusUnits.FindAsync(id);
                if (unit != null)
                {
                    unit.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveTopic(int unitId, int topicId, string topicName, int expectedPeriods)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                int userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : 1;

                if (topicId == 0)
                {
                    var topic = new TblSyllabusTopic
                    {
                        UnitId = unitId,
                        TopicName = topicName,
                        ExpectedPeriods = expectedPeriods,
                        IsActive = true,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now
                    };
                    _context.TblSyllabusTopics.Add(topic);
                }
                else
                {
                    var topic = await _context.TblSyllabusTopics.FindAsync(topicId);
                    if (topic != null)
                    {
                        topic.TopicName = topicName;
                        topic.ExpectedPeriods = expectedPeriods;
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTopic(int id)
        {
            try
            {
                var topic = await _context.TblSyllabusTopics.FindAsync(id);
                if (topic != null)
                {
                    topic.IsActive = false;
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
