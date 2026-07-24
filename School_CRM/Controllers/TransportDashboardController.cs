using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportDashboardController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportDashboardController(LibmanagementContext context)
            => _context = context;

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 1;

        public async Task<IActionResult> Index()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .ToListAsync();
            return View();
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var thirtyDaysLater = today.AddDays(30);

                var totalVehicles = await _context.TblTrnVehicles
                    .CountAsync(v => v.IsActive);

                var scheduledToday = await _context.TblTrnTrips
                    .CountAsync(t => t.TripDate == today && t.TripStatus == "Scheduled" && t.IsActive);

                var inProgressToday = await _context.TblTrnTrips
                    .CountAsync(t => t.TripDate == today && t.TripStatus == "InProgress" && t.IsActive);

                var completedToday = await _context.TblTrnTrips
                    .CountAsync(t => t.TripDate == today && t.TripStatus == "Completed" && t.IsActive);

                // Vehicles with overdue maintenance (NextServiceDueDate < today)
                var overdueService = await _context.TblTrnMaintenanceLogs
                    .Where(m => m.IsActive && m.NextServiceDueDate.HasValue && m.NextServiceDueDate.Value < today)
                    .Select(m => m.VehicleId)
                    .Distinct()
                    .CountAsync();

                // Vehicles with insurance expiring within 30 days
                var expiringInsurance30Days = await _context.TblTrnVehicles
                    .CountAsync(v => v.IsActive && v.InsuranceExpiry.HasValue
                        && v.InsuranceExpiry.Value >= today
                        && v.InsuranceExpiry.Value <= thirtyDaysLater);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        totalVehicles,
                        scheduledToday,
                        inProgressToday,
                        completedToday,
                        overdueService,
                        expiringInsurance30Days
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetTodayTrips()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                var trips = await _context.TblTrnTrips
                    .Where(t => t.TripDate == today && t.IsActive)
                    .Include(t => t.Route)
                    .Include(t => t.Assignment)
                        .ThenInclude(a => a!.Vehicle)
                    .Include(t => t.Assignment)
                        .ThenInclude(a => a!.Driver)
                    .OrderBy(t => t.TripType)
                    .Select(t => new
                    {
                        t.TripId,
                        RouteName = t.Route.RouteName,
                        TripType = t.TripType,
                        VehicleNo = t.Assignment != null ? t.Assignment.Vehicle.RegistrationNumber : "-",
                        DriverName = t.Assignment != null ? t.Assignment.Driver.DriverName : "-",
                        t.TripStatus,
                        t.AdherenceStatus,
                        ActualStart = t.ActualStartTime.HasValue ? t.ActualStartTime.Value.ToString("hh:mm tt") : "-",
                        ActualEnd = t.ActualEndTime.HasValue ? t.ActualEndTime.Value.ToString("hh:mm tt") : "-"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = trips });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetRouteCapacityOverview(int? sessionId)
        {
            try
            {
                if (!sessionId.HasValue || sessionId == 0)
                {
                    sessionId = await _context.TblAcademicSessions
                        .Where(s => s.IsActive == true)
                        .OrderByDescending(s => s.SessionId)
                        .Select(s => s.SessionId)
                        .FirstOrDefaultAsync();
                }

                var routes = await _context.TblTrnRoutes
                    .Where(r => r.SessionId == sessionId && r.IsActive)
                    .Select(r => new
                    {
                        r.RouteId,
                        r.RouteName,
                        r.MaxStudentCapacity,
                        AssignedCount = r.TblTrnStudentAssignments
                            .Count(sa => sa.SessionId == sessionId && sa.IsActive && sa.AssignmentStatus == "Active")
                    })
                    .ToListAsync();

                var result = routes.Select(r => new
                {
                    r.RouteId,
                    r.RouteName,
                    Capacity = r.MaxStudentCapacity,
                    Assigned = r.AssignedCount,
                    Available = r.MaxStudentCapacity - r.AssignedCount,
                    UtilisationPct = r.MaxStudentCapacity > 0
                        ? Math.Round((double)r.AssignedCount / r.MaxStudentCapacity * 100, 1)
                        : 0
                });

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
