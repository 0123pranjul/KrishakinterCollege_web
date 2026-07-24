using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportRouteController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportRouteController(LibmanagementContext context)
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
        public async Task<IActionResult> GetAll(int? sessionId)
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
                    .Include(r => r.Session)
                    .Select(r => new
                    {
                        r.RouteId,
                        r.RouteName,
                        r.MaxStudentCapacity,
                        r.Description,
                        SessionName = r.Session.SessionName,
                        AssignedCount = r.TblTrnStudentAssignments
                            .Count(sa => sa.SessionId == sessionId && sa.IsActive && sa.AssignmentStatus == "Active"),
                        StopCount = r.TblTrnRouteStops.Count(s => s.IsActive)
                    })
                    .ToListAsync();

                var result = routes.Select(r => new
                {
                    r.RouteId,
                    r.RouteName,
                    r.MaxStudentCapacity,
                    r.Description,
                    r.SessionName,
                    r.AssignedCount,
                    r.StopCount,
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

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .ToListAsync();

            var model = id == 0
                ? new TblTrnRoute { IsActive = true, MaxStudentCapacity = 40 }
                : await _context.TblTrnRoutes.FindAsync(id)
                  ?? new TblTrnRoute { IsActive = true, MaxStudentCapacity = 40 };

            return PartialView("_RouteForm", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblTrnRoute model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnRoutes.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Route added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnRoutes.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Route not found." });

                    existing.RouteName = model.RouteName;
                    existing.SessionId = model.SessionId;
                    existing.MaxStudentCapacity = model.MaxStudentCapacity;
                    existing.Description = model.Description;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Route updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var route = await _context.TblTrnRoutes.FindAsync(id);
                if (route == null)
                    return Json(new { success = false, message = "Route not found." });

                route.IsActive = false;
                route.UpdatedBy = CurrentUserId;
                route.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Route deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── STOPS ─────────────────────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetStops(int routeId)
        {
            try
            {
                var stops = await _context.TblTrnRouteStops
                    .Where(s => s.RouteId == routeId && s.IsActive)
                    .OrderBy(s => s.StopOrder)
                    .Select(s => new
                    {
                        s.StopId,
                        s.StopName,
                        s.StopOrder,
                        ArrivalTime = s.ScheduledArrivalTime.ToString("hh\\:mm"),
                        DepartureTime = s.ScheduledDepartureTime.ToString("hh\\:mm"),
                        s.FareAmount,
                        s.Latitude,
                        s.Longitude,
                        s.Landmark
                    })
                    .ToListAsync();

                return Json(new { success = true, data = stops });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveStop([FromBody] TblTrnRouteStop model)
        {
            try
            {
                if (model.StopId == 0)
                {
                    // Auto-assign order if not specified
                    if (model.StopOrder == 0)
                    {
                        var maxOrder = await _context.TblTrnRouteStops
                            .Where(s => s.RouteId == model.RouteId && s.IsActive)
                            .MaxAsync(s => (short?)s.StopOrder) ?? 0;
                        model.StopOrder = (short)(maxOrder + 1);
                    }
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnRouteStops.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Stop added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnRouteStops.FindAsync(model.StopId);
                    if (existing == null)
                        return Json(new { success = false, message = "Stop not found." });

                    existing.StopName = model.StopName;
                    existing.StopOrder = model.StopOrder;
                    existing.ScheduledArrivalTime = model.ScheduledArrivalTime;
                    existing.ScheduledDepartureTime = model.ScheduledDepartureTime;
                    existing.FareAmount = model.FareAmount;
                    existing.Latitude = model.Latitude;
                    existing.Longitude = model.Longitude;
                    existing.Landmark = model.Landmark;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Stop updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteStop(int stopId)
        {
            try
            {
                var hasStudents = await _context.TblTrnStudentAssignments
                    .AnyAsync(sa => sa.StopId == stopId && sa.IsActive);
                if (hasStudents)
                    return Json(new { success = false, message = "Cannot delete stop: students are assigned to it." });

                var stop = await _context.TblTrnRouteStops.FindAsync(stopId);
                if (stop == null)
                    return Json(new { success = false, message = "Stop not found." });

                stop.IsActive = false;
                stop.UpdatedBy = CurrentUserId;
                stop.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Stop deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ReorderStops([FromBody] List<StopReorderDto> items)
        {
            try
            {
                foreach (var item in items)
                {
                    var stop = await _context.TblTrnRouteStops.FindAsync(item.StopId);
                    if (stop != null)
                    {
                        stop.StopOrder = (short)item.NewOrder;
                        stop.UpdatedBy = CurrentUserId;
                        stop.UpdatedDate = DateTime.Now;
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Stops reordered successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class StopReorderDto
    {
        public int StopId { get; set; }
        public int NewOrder { get; set; }
    }
}
