using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace School_CRM.Controllers
{
    public class LessonPlanReviewController : Controller
    {
        private readonly LibmanagementContext _context;

        public LessonPlanReviewController(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Fetch all Pending lesson plans for the Principal/HOD to review
            var plans = await _context.TblLessonPlans
                .Include(p => p.Class)
                .Include(p => p.Subject)
                .Include(p => p.Employee)
                .Where(p => p.Status == "Pending" || p.Status == "Approved" || p.Status == "Rejected")
                .OrderBy(p => p.Status == "Pending" ? 0 : 1)
                .ThenByDescending(p => p.CreatedDate)
                .ToListAsync();

            return View(plans);
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(int id)
        {
            var plan = await _context.TblLessonPlans
                .Include(p => p.Class)
                .Include(p => p.Subject)
                .Include(p => p.Employee)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (plan == null) return NotFound();

            return Json(new
            {
                id = plan.Id,
                title = plan.PlanTitle,
                teacher = plan.Employee?.Name,
                className = plan.Class?.ClassName,
                subject = plan.Subject?.SubjectName,
                duration = $"{plan.StartDate:dd MMM yyyy} to {plan.EndDate:dd MMM yyyy}",
                objectives = plan.Objectives,
                methodology = plan.TeachingMethod,
                materials = plan.RequiredMaterials,
                status = plan.Status,
                remarks = plan.ReviewRemarks
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status, string remarks)
        {
            try
            {
                var userIdStr = Request.Cookies["userId"];
                int userId = !string.IsNullOrEmpty(userIdStr) ? int.Parse(userIdStr) : 1;

                var plan = await _context.TblLessonPlans.FindAsync(id);
                if (plan != null)
                {
                    plan.Status = status;
                    plan.ReviewRemarks = remarks;
                    plan.ReviewedBy = userId;
                    await _context.SaveChangesAsync();
                    return Json(new { success = true });
                }
                return Json(new { success = false, message = "Plan not found." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
