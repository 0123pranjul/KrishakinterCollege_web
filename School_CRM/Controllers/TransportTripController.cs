using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportTripController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportTripController(LibmanagementContext context)
            => _context = context;

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 1;

        public async Task<IActionResult> Index()
        {
            ViewBag.Routes = await _context.TblTrnRoutes
                .Where(r => r.IsActive)
                .OrderBy(r => r.RouteName)
                .ToListAsync();
            return View();
        }

        public async Task<IActionResult> Schedule()
        {
            ViewBag.Routes = await _context.TblTrnRoutes
                .Where(r => r.IsActive)
                .OrderBy(r => r.RouteName)
                .ToListAsync();
            return View();
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetAll(string? date, int? routeId)
        {
            try
            {
                DateOnly filterDate = string.IsNullOrEmpty(date)
                    ? DateOnly.FromDateTime(DateTime.Today)
                    : DateOnly.Parse(date);

                var query = _context.TblTrnTrips
                    .Where(t => t.TripDate == filterDate && t.IsActive)
                    .Include(t => t.Route)
                    .Include(t => t.Assignment)
                        .ThenInclude(a => a!.Vehicle)
                    .Include(t => t.Assignment)
                        .ThenInclude(a => a!.Driver)
                    .AsQueryable();

                if (routeId.HasValue && routeId > 0)
                    query = query.Where(t => t.RouteId == routeId);

                var trips = await query
                    .OrderBy(t => t.TripType)
                    .ThenBy(t => t.TripStatus)
                    .Select(t => new
                    {
                        t.TripId,
                        RouteName = t.Route.RouteName,
                        t.TripType,
                        t.TripStatus,
                        t.AdherenceStatus,
                        VehicleNo = t.Assignment != null ? t.Assignment.Vehicle.RegistrationNumber : "-",
                        DriverName = t.Assignment != null ? t.Assignment.Driver.DriverName : "-",
                        TripDate = t.TripDate.ToString("dd MMM yyyy"),
                        ActualStart = t.ActualStartTime.HasValue ? t.ActualStartTime.Value.ToString("hh:mm tt") : "-",
                        ActualEnd = t.ActualEndTime.HasValue ? t.ActualEndTime.Value.ToString("hh:mm tt") : "-",
                        t.Remarks
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
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            ViewBag.Routes = await _context.TblTrnRoutes
                .Where(r => r.IsActive)
                .OrderBy(r => r.RouteName)
                .ToListAsync();

            TblTrnTrip model;
            string? warning = null;

            if (id == 0)
            {
                model = new TblTrnTrip
                {
                    TripDate = DateOnly.FromDateTime(DateTime.Today),
                    TripType = "Morning",
                    TripStatus = "Scheduled",
                    IsActive = true,
                    SecureToken = Guid.NewGuid().ToString("N")
                };
            }
            else
            {
                model = await _context.TblTrnTrips.FindAsync(id)
                        ?? new TblTrnTrip { TripDate = DateOnly.FromDateTime(DateTime.Today), IsActive = true };
            }

            // Warning: check if selected route has a vehicle assignment
            if (model.RouteId > 0)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var hasAssignment = await _context.TblTrnVehicleAssignments
                    .AnyAsync(a => a.RouteId == model.RouteId && a.IsActive
                                && a.AssignedFrom <= model.TripDate
                                && a.AssignedTo >= model.TripDate);
                if (!hasAssignment)
                    warning = "Warning: No vehicle assignment found for this route on the trip date.";
            }

            ViewBag.Warning = warning;
            return PartialView("_TripForm", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblTrnTrip model)
        {
            try
            {
                // Check vehicle assignment warning
                var hasAssignment = await _context.TblTrnVehicleAssignments
                    .AnyAsync(a => a.RouteId == model.RouteId && a.IsActive
                                && a.AssignedFrom <= model.TripDate
                                && a.AssignedTo >= model.TripDate);

                if (id == 0)
                {
                    // Duplicate check
                    var dupExists = await _context.TblTrnTrips
                        .AnyAsync(t => t.RouteId == model.RouteId
                                    && t.TripDate == model.TripDate
                                    && t.TripType == model.TripType
                                    && t.IsActive);
                    if (dupExists)
                        return Json(new { success = false, message = "A trip already exists for this route, date, and type." });

                    // Auto-set assignment
                    var assignment = await _context.TblTrnVehicleAssignments
                        .FirstOrDefaultAsync(a => a.RouteId == model.RouteId && a.IsActive
                                               && a.AssignedFrom <= model.TripDate
                                               && a.AssignedTo >= model.TripDate);
                    model.AssignmentId = assignment?.AssignmentId;

                    model.SecureToken = Guid.NewGuid().ToString("N");
                    model.TripStatus = "Scheduled";
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnTrips.Add(model);
                    await _context.SaveChangesAsync();

                    return Json(new
                    {
                        success = true,
                        message = "Trip created successfully." + (!hasAssignment ? " Note: No vehicle assignment found for this route on the trip date." : "")
                    });
                }
                else
                {
                    var existing = await _context.TblTrnTrips.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Trip not found." });

                    existing.RouteId = model.RouteId;
                    existing.TripDate = model.TripDate;
                    existing.TripType = model.TripType;
                    existing.Remarks = model.Remarks;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Trip updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> StartTrip(int tripId)
        {
            try
            {
                var trip = await _context.TblTrnTrips
                    .Include(t => t.Route)
                    .FirstOrDefaultAsync(t => t.TripId == tripId);

                if (trip == null)
                    return Json(new { success = false, message = "Trip not found." });

                if (trip.TripStatus != "Scheduled")
                    return Json(new { success = false, message = "Only scheduled trips can be started." });

                trip.TripStatus = "InProgress";
                trip.ActualStartTime = DateTime.Now;
                trip.UpdatedBy = CurrentUserId;
                trip.UpdatedDate = DateTime.Now;

                // Auto-create boarding logs for all students on this route in this session
                var sessionId = trip.Route.SessionId;
                var studentAssignments = await _context.TblTrnStudentAssignments
                    .Where(sa => sa.RouteId == trip.RouteId && sa.SessionId == sessionId
                              && sa.IsActive && sa.AssignmentStatus == "Active")
                    .ToListAsync();

                // Remove existing boarding logs for this trip if re-starting
                var existingLogs = await _context.TblTrnTripBoardingLogs
                    .Where(bl => bl.TripId == tripId)
                    .ToListAsync();
                _context.TblTrnTripBoardingLogs.RemoveRange(existingLogs);

                foreach (var sa in studentAssignments)
                {
                    _context.TblTrnTripBoardingLogs.Add(new TblTrnTripBoardingLog
                    {
                        TripId = tripId,
                        StudentId = sa.StudentId,
                        StopId = sa.StopId,
                        BoardingStatus = "Pending"
                    });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Trip started. Boarding logs created." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CompleteTrip(int tripId)
        {
            try
            {
                var trip = await _context.TblTrnTrips.FindAsync(tripId);
                if (trip == null)
                    return Json(new { success = false, message = "Trip not found." });

                if (trip.TripStatus != "InProgress")
                    return Json(new { success = false, message = "Only in-progress trips can be completed." });

                trip.TripStatus = "Completed";
                trip.ActualEndTime = DateTime.Now;
                trip.UpdatedBy = CurrentUserId;
                trip.UpdatedDate = DateTime.Now;

                // Calculate adherence status
                if (trip.ActualStartTime.HasValue)
                {
                    var actualDuration = (DateTime.Now - trip.ActualStartTime.Value).TotalMinutes;
                    // Simple adherence: if ended within reasonable time, mark as OnTime
                    trip.AdherenceStatus = actualDuration <= 120 ? "OnTime" : "Delayed";
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Trip completed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetBoardingStatus(int tripId)
        {
            try
            {
                var logs = await _context.TblTrnTripBoardingLogs
                    .Where(bl => bl.TripId == tripId)
                    .Include(bl => bl.Student)
                    .Include(bl => bl.Stop)
                    .OrderBy(bl => bl.Stop.StopOrder)
                    .ThenBy(bl => bl.Student.StudentName)
                    .Select(bl => new
                    {
                        bl.Id,
                        StudentName = bl.Student.StudentName,
                        StopName = bl.Stop.StopName,
                        StopOrder = bl.Stop.StopOrder,
                        bl.BoardingStatus,
                        UpdatedAt = bl.UpdatedAt.HasValue ? bl.UpdatedAt.Value.ToString("hh:mm tt") : "-"
                    })
                    .ToListAsync();

                // Group by stop
                var grouped = logs
                    .GroupBy(l => new { l.StopName, l.StopOrder })
                    .OrderBy(g => g.Key.StopOrder)
                    .Select(g => new
                    {
                        StopName = g.Key.StopName,
                        StopOrder = g.Key.StopOrder,
                        Students = g.ToList()
                    });

                return Json(new { success = true, data = grouped });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateBoarding(int boardingLogId, string status)
        {
            try
            {
                var log = await _context.TblTrnTripBoardingLogs.FindAsync(boardingLogId);
                if (log == null)
                    return Json(new { success = false, message = "Boarding log not found." });

                log.BoardingStatus = status;
                log.UpdatedAt = DateTime.Now;
                log.UpdatedBy = CurrentUserId;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Boarding status updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetTrackingData(int tripId)
        {
            try
            {
                var trip = await _context.TblTrnTrips
                    .Include(t => t.Route)
                        .ThenInclude(r => r.TblTrnRouteStops)
                    .FirstOrDefaultAsync(t => t.TripId == tripId);

                if (trip == null)
                    return Json(new { success = false, message = "Trip not found." });

                var stops = trip.Route.TblTrnRouteStops
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.StopOrder)
                    .ToList();

                // Check for recent GPS update (within last 5 minutes)
                var fiveMinAgo = DateTime.Now.AddMinutes(-5);
                var latestGps = await _context.TblTrnGpsUpdates
                    .Where(g => g.TripId == tripId && g.ReceivedAt >= fiveMinAgo)
                    .OrderByDescending(g => g.ReceivedAt)
                    .FirstOrDefaultAsync();

                object? positionData = null;
                string gpsMode = "Estimated";

                if (latestGps != null)
                {
                    // Live GPS available
                    gpsMode = "Live";
                    positionData = new
                    {
                        Latitude = (double)latestGps.Latitude,
                        Longitude = (double)latestGps.Longitude,
                        ReceivedAt = latestGps.ReceivedAt.ToString("hh:mm:ss tt"),
                        Label = "Live GPS Position"
                    };
                }
                else if (trip.ActualStartTime.HasValue && stops.Any())
                {
                    // Schedule-based position estimate
                    var now = DateTime.Now;
                    var actualStart = trip.ActualStartTime.Value;
                    var offsetMinutes = (now - actualStart).TotalMinutes;

                    // First stop's scheduled departure is the reference
                    var firstDep = stops.First().ScheduledDepartureTime;
                    var firstDepMinutes = firstDep.Hour * 60 + firstDep.Minute;

                    // Find which segment the bus is in
                    TblTrnRouteStop? lastStop = null;
                    TblTrnRouteStop? nextStop = null;
                    double fraction = 0;

                    for (int i = 0; i < stops.Count - 1; i++)
                    {
                        var s1 = stops[i];
                        var s2 = stops[i + 1];
                        var dep1 = s1.ScheduledDepartureTime.Hour * 60 + s1.ScheduledDepartureTime.Minute;
                        var arr2 = s2.ScheduledArrivalTime.Hour * 60 + s2.ScheduledArrivalTime.Minute;

                        var schedOffset = dep1 - firstDepMinutes;
                        var segDuration = arr2 - dep1;

                        if (offsetMinutes >= schedOffset && segDuration > 0)
                        {
                            lastStop = s1;
                            nextStop = s2;
                            fraction = Math.Min(1.0, (offsetMinutes - schedOffset) / segDuration);
                        }
                    }

                    if (lastStop == null) lastStop = stops.First();
                    if (nextStop == null) nextStop = stops.Last();

                    // Interpolate position if coords available
                    double? estimatedLat = null;
                    double? estimatedLng = null;
                    if (lastStop.Latitude.HasValue && lastStop.Longitude.HasValue
                     && nextStop.Latitude.HasValue && nextStop.Longitude.HasValue)
                    {
                        estimatedLat = (double)lastStop.Latitude.Value
                            + fraction * ((double)nextStop.Latitude.Value - (double)lastStop.Latitude.Value);
                        estimatedLng = (double)lastStop.Longitude.Value
                            + fraction * ((double)nextStop.Longitude.Value - (double)lastStop.Longitude.Value);
                    }

                    positionData = new
                    {
                        Latitude = estimatedLat,
                        Longitude = estimatedLng,
                        Label = $"Between {lastStop.StopName} and {nextStop.StopName} (~{Math.Round(fraction * 100)}%)",
                        LastStopName = lastStop.StopName,
                        NextStopName = nextStop.StopName,
                        FractionInSegment = Math.Round(fraction, 2)
                    };
                }

                var stopsData = stops.Select(s => new
                {
                    s.StopId,
                    s.StopName,
                    s.StopOrder,
                    ArrivalTime = s.ScheduledArrivalTime.ToString("hh\\:mm"),
                    Latitude = s.Latitude.HasValue ? (double?)s.Latitude.Value : null,
                    Longitude = s.Longitude.HasValue ? (double?)s.Longitude.Value : null
                });

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        TripId = tripId,
                        RouteName = trip.Route.RouteName,
                        trip.TripStatus,
                        GpsMode = gpsMode,
                        Position = positionData,
                        Stops = stopsData
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── AUTO GENERATE TODAY'S TRIPS ──────────────────────────────────────
        // Ek click mein aaj ke liye saari routes ke Morning + Evening trips create karo
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GenerateTodayTrips()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);

                // Saare active vehicle assignments jo aaj valid hain
                var assignments = await _context.TblTrnVehicleAssignments
                    .Include(a => a.Route)
                    .Where(a => a.IsActive
                             && a.AssignedFrom <= today
                             && a.AssignedTo   >= today)
                    .ToListAsync();

                if (!assignments.Any())
                    return Json(new { success = false, message = "Koi active vehicle assignment nahi mili aaj ki date ke liye." });

                int created = 0, skipped = 0;
                var errors  = new List<string>();

                foreach (var assignment in assignments)
                {
                    foreach (var tripType in new[] { "Morning", "Evening" })
                    {
                        // Duplicate check
                        bool exists = await _context.TblTrnTrips.AnyAsync(t =>
                            t.RouteId  == assignment.RouteId
                         && t.TripDate == today
                         && t.TripType == tripType
                         && t.IsActive);

                        if (exists) { skipped++; continue; }

                        _context.TblTrnTrips.Add(new TblTrnTrip
                        {
                            RouteId      = assignment.RouteId,
                            AssignmentId = assignment.AssignmentId,
                            TripDate     = today,
                            TripType     = tripType,
                            TripStatus   = "Scheduled",
                            SecureToken  = Guid.NewGuid().ToString("N"),
                            IsActive     = true,
                            CreatedBy    = CurrentUserId,
                            CreatedDate  = DateTime.Now
                        });
                        created++;
                    }
                }

                await _context.SaveChangesAsync();

                var msg = created > 0
                    ? $"{created} trip(s) create ki gayi."
                    : "Aaj ki saari trips already exist hain.";
                if (skipped > 0) msg += $" {skipped} already exist thi (skip ki)";

                return Json(new
                {
                    success = true,
                    message = msg,
                    created,
                    skipped
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // ── WEEKLY SCHEDULE ──────────────────────────────────────────────────────────
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetWeeklySchedule(int? routeId)
        {
            try
            {
                var query = _context.TblTrnWeeklySchedules
                    .Where(s => s.IsActive)
                    .Include(s => s.Route)
                    .AsQueryable();

                if (routeId.HasValue && routeId > 0)
                    query = query.Where(s => s.RouteId == routeId);

                var schedules = await query
                    .OrderBy(s => s.Route.RouteName)
                    .ThenBy(s => s.DayOfWeek)
                    .Select(s => new {
                        s.Id,
                        s.RouteId,
                        RouteName  = s.Route.RouteName,
                        s.DayOfWeek,
                        DayName    = s.DayOfWeek == 1 ? "Monday"
                                   : s.DayOfWeek == 2 ? "Tuesday"
                                   : s.DayOfWeek == 3 ? "Wednesday"
                                   : s.DayOfWeek == 4 ? "Thursday"
                                   : s.DayOfWeek == 5 ? "Friday"
                                   : s.DayOfWeek == 6 ? "Saturday" : "Sunday",
                        s.TripType,
                        s.IsActive
                    })
                    .ToListAsync();

                return Json(new { success = true, data = schedules });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        //[HttpGet]
        //[IgnoreAntiforgeryToken]
        //public async Task<IActionResult> Schedule()
        //{
        //    ViewBag.Routes = await _context.TblTrnRoutes
        //        .Where(r => r.IsActive)
        //        .OrderBy(r => r.RouteName)
        //        .ToListAsync();
        //    return View();
        //}

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveWeeklySchedule([FromBody] WeeklyScheduleDto dto)
        {
            try
            {
                // Delete existing schedules for this route then recreate
                var existing = await _context.TblTrnWeeklySchedules
                    .Where(s => s.RouteId == dto.RouteId)
                    .ToListAsync();
                _context.TblTrnWeeklySchedules.RemoveRange(existing);

                foreach (var entry in dto.Entries)
                {
                    _context.TblTrnWeeklySchedules.Add(new TblTrnWeeklySchedule
                    {
                        RouteId     = dto.RouteId,
                        DayOfWeek   = entry.DayOfWeek,
                        TripType    = entry.TripType,
                        IsActive    = true,
                        CreatedBy   = CurrentUserId,
                        CreatedDate = DateTime.Now
                    });
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Weekly schedule saved!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── GENERATE TRIPS FOR DATE RANGE ────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GenerateScheduledTrips([FromBody] GenerateTripsDto dto)
        {
            try
            {
                var fromDate = DateOnly.Parse(dto.FromDate);
                var toDate   = DateOnly.Parse(dto.ToDate);

                if (toDate < fromDate)
                    return Json(new { success = false, message = "ToDate must be >= FromDate" });
                if ((toDate.DayNumber - fromDate.DayNumber) > 30)
                    return Json(new { success = false, message = "Max 30 days range allowed." });

                var schedules = await _context.TblTrnWeeklySchedules
                    .Where(s => s.IsActive && (!dto.RouteId.HasValue || s.RouteId == dto.RouteId))
                    .ToListAsync();

                if (!schedules.Any())
                    return Json(new { success = false, message = "Koi weekly schedule define nahi hai. Pehle schedule set karo." });

                int created = 0, skipped = 0;

                for (var date = fromDate; date <= toDate; date = date.AddDays(1))
                {
                    // DotNet: 0=Sun,1=Mon...6=Sat  |  Our system: 1=Mon...6=Sat (Sunday not a school day)
                    int dayNum = date.DayOfWeek == DayOfWeek.Sunday ? 0
                        : (int)date.DayOfWeek; // Mon=1, Tue=2... Sat=6

                    if (dayNum == 0) continue; // Skip Sunday

                    var todaySchedules = schedules.Where(s => s.DayOfWeek == dayNum).ToList();

                    foreach (var sched in todaySchedules)
                    {
                        var tripTypes = sched.TripType == "Both"
                            ? new[] { "Morning", "Evening" }
                            : new[] { sched.TripType };

                        // Get active vehicle assignment for this route on this date
                        var assignment = await _context.TblTrnVehicleAssignments
                            .FirstOrDefaultAsync(a =>
                                a.RouteId      == sched.RouteId
                             && a.IsActive
                             && a.AssignedFrom <= date
                             && a.AssignedTo   >= date);

                        foreach (var tripType in tripTypes)
                        {
                            bool exists = await _context.TblTrnTrips.AnyAsync(t =>
                                t.RouteId  == sched.RouteId
                             && t.TripDate == date
                             && t.TripType == tripType
                             && t.IsActive);

                            if (exists) { skipped++; continue; }

                            _context.TblTrnTrips.Add(new TblTrnTrip
                            {
                                RouteId      = sched.RouteId,
                                AssignmentId = assignment?.AssignmentId,
                                TripDate     = date,
                                TripType     = tripType,
                                TripStatus   = "Scheduled",
                                SecureToken  = Guid.NewGuid().ToString("N"),
                                IsActive     = true,
                                CreatedBy    = CurrentUserId,
                                CreatedDate  = DateTime.Now
                            });
                            created++;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new {
                    success = true,
                    message = $"{created} trip(s) generated. {skipped} already existed (skipped).",
                    created, skipped
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── CANCEL TRIP ───────────────────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CancelTrip(int tripId, string? reason)
        {
            try
            {
                var trip = await _context.TblTrnTrips.FindAsync(tripId);
                if (trip == null) return Json(new { success = false, message = "Trip not found." });
                if (trip.TripStatus == "InProgress")
                    return Json(new { success = false, message = "InProgress trip cancel nahi ki ja sakti." });

                trip.TripStatus  = "Cancelled";
                trip.Remarks     = reason ?? "Admin cancelled";
                trip.UpdatedBy   = CurrentUserId;
                trip.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Trip cancelled." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── EDIT TRIP REMARKS/NOTES ───────────────────────────────────────────────────
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateTripRemarks(int tripId, string remarks)
        {
            try
            {
                var trip = await _context.TblTrnTrips.FindAsync(tripId);
                if (trip == null) return Json(new { success = false, message = "Trip not found." });

                trip.Remarks     = remarks;
                trip.UpdatedBy   = CurrentUserId;
                trip.UpdatedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Remarks updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GPS endpoint - token based, no auth required
        [AllowAnonymous]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateGps(string token, [FromBody] GpsUpdateDto dto)
        {
            try
            {
                var trip = await _context.TblTrnTrips
                    .FirstOrDefaultAsync(t => t.SecureToken == token && t.IsActive);

                if (trip == null)
                    return Json(new { success = false, message = "Invalid token." });

                if (trip.TripStatus != "InProgress")
                    return Json(new { success = false, message = "Trip is not in progress." });

                // Validate lat/lng ranges
                if (dto.Latitude < -90 || dto.Latitude > 90 || dto.Longitude < -180 || dto.Longitude > 180)
                    return Json(new { success = false, message = "Invalid coordinates." });

                _context.TblTrnGpsUpdates.Add(new TblTrnGpsUpdate
                {
                    TripId = trip.TripId,
                    Latitude = (decimal)dto.Latitude,
                    Longitude = (decimal)dto.Longitude,
                    ReceivedAt = DateTime.Now,
                    DeviceTimestamp = dto.DeviceTimestamp
                });
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "GPS updated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public async Task<IActionResult> Track(int id)
        {
            var trip = await _context.TblTrnTrips
                .Include(t => t.Route)
                .FirstOrDefaultAsync(t => t.TripId == id);

            if (trip == null) return NotFound();

            ViewBag.Trip = trip;
            return View();
        }
    }

    public class GpsUpdateDto
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public DateTime? DeviceTimestamp { get; set; }
    }

    public class WeeklyScheduleDto
    {
        public int RouteId { get; set; }
        public List<ScheduleEntry> Entries { get; set; } = new();
    }

    public class ScheduleEntry
    {
        public byte DayOfWeek { get; set; }   // 1=Mon...6=Sat
        public string TripType { get; set; } = "Both";
    }

    public class GenerateTripsDto
    {
        public string FromDate { get; set; } = "";
        public string ToDate   { get; set; } = "";
        public int?   RouteId  { get; set; }
    }
}
