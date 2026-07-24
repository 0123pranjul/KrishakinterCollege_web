using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;
using System.Linq;
using System.Threading.Tasks;

namespace School_CRM.Controllers
{
    public class StudyCenterDashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public StudyCenterDashboardController(LibmanagementContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            int totalMaterials = await _context.TblStudyMaterials.CountAsync(m => m.IsActive == true);
            int totalAssignments = await _context.TblAssignments.CountAsync(a => a.IsActive == true);
            int totalSyllabusUnits = await _context.TblSyllabusUnits.CountAsync(u => u.IsActive == true);

            var recentLogs = await _context.TblClassworkLogs
                .Include(l => l.Class)
                .Include(l => l.Section)
                .Include(l => l.Subject)
                .Include(l => l.Employee)
                .OrderByDescending(l => l.LogDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalMaterials = totalMaterials;
            ViewBag.TotalAssignments = totalAssignments;
            ViewBag.TotalSyllabusUnits = totalSyllabusUnits;

            return View(recentLogs);
        }
    }
}
