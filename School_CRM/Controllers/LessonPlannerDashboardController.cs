using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;

namespace School_CRM.Controllers
{
    public class LessonPlannerDashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public LessonPlannerDashboardController(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int totalPlans = await _context.TblLessonPlans.CountAsync();
            int pendingPlans = await _context.TblLessonPlans.CountAsync(p => p.Status == "Pending");
            int approvedPlans = await _context.TblLessonPlans.CountAsync(p => p.Status == "Approved");
            int rejectedPlans = await _context.TblLessonPlans.CountAsync(p => p.Status == "Rejected");

            var recentSubmissions = await _context.TblLessonPlans
                .Include(p => p.Employee)
                .Include(p => p.Subject)
                .Where(p => p.Status == "Pending" || p.Status == "Approved")
                .OrderByDescending(p => p.CreatedDate)
                .Take(5)
                .ToListAsync();

            var coverages = await _context.TblLessonCoverages.ToListAsync();
            double avgCoverage = 0;
            if (coverages.Any())
            {
                avgCoverage = coverages.Average(c => c.PercentageCompleted);
            }

            ViewBag.TotalPlans = totalPlans;
            ViewBag.PendingPlans = pendingPlans;
            ViewBag.ApprovedPlans = approvedPlans;
            ViewBag.RejectedPlans = rejectedPlans;
            ViewBag.AvgCoverage = System.Math.Round(avgCoverage, 1);

            return View(recentSubmissions);
        }
    }
}
