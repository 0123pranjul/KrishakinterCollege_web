using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace School_CRM.Controllers
{
    public class LessonCoverageController : Controller
    {
        private readonly LibmanagementContext _context;

        public LessonCoverageController(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // For testing, if employee login isn't active, we just show all approved plans.
            // Ideally we filter by `p.EmployeeId == userId`
            var plans = await _context.TblLessonPlans
                .Include(p => p.Class)
                .Include(p => p.Subject)
                .Include(p => p.TblLessonCoverages)
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();

            return View(plans);
        }

        [HttpPost]
        public async Task<IActionResult> SaveCoverage(int planId, string dateCovered, int percentage, string notes)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                int userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : 1;

                var coverage = new TblLessonCoverage
                {
                    LessonPlanId = planId,
                    DateCovered = DateOnly.Parse(dateCovered),
                    PercentageCompleted = percentage,
                    TeacherNotes = notes,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                };

                _context.TblLessonCoverages.Add(coverage);
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
