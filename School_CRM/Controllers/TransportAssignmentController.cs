using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportAssignmentController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportAssignmentController(LibmanagementContext context)
            => _context = context;

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 1;

        public async Task<IActionResult> Index()
        {
            ViewBag.Sessions = await _context.TblAcademicSessions
                .Where(s => s.IsActive == true)
                .OrderByDescending(s => s.SessionId)
                .ToListAsync();

            ViewBag.Routes = await _context.TblTrnRoutes
                .Where(r => r.IsActive)
                .OrderBy(r => r.RouteName)
                .ToListAsync();

            ViewBag.Vehicles = await _context.TblTrnVehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();

            ViewBag.Drivers = await _context.TblTrnDrivers
                .Where(d => d.IsActive)
                .OrderBy(d => d.DriverName)
                .ToListAsync();

            ViewBag.Conductors = await _context.TblTrnConductors
                .Where(c => c.IsActive)
                .OrderBy(c => c.ConductorName)
                .ToListAsync();

            return View();
        }

        // ── VEHICLE ASSIGNMENTS ───────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetVehicleAssignments(int? sessionId)
        {
            try
            {
                var query = _context.TblTrnVehicleAssignments
                    .Where(a => a.IsActive)
                    .Include(a => a.Route)
                    .Include(a => a.Vehicle)
                    .Include(a => a.Driver)
                    .Include(a => a.Conductor)
                    .AsQueryable();

                if (sessionId.HasValue && sessionId > 0)
                    query = query.Where(a => a.Route.SessionId == sessionId);

                var assignments = await query
                    .OrderByDescending(a => a.AssignedFrom)
                    .Select(a => new
                    {
                        a.AssignmentId,
                        RouteName = a.Route.RouteName,
                        VehicleNo = a.Vehicle.RegistrationNumber,
                        VehicleType = a.Vehicle.VehicleType,
                        DriverName = a.Driver.DriverName,
                        ConductorName = a.Conductor != null ? a.Conductor.ConductorName : "-",
                        AssignedFrom = a.AssignedFrom.ToString("dd MMM yyyy"),
                        AssignedTo = a.AssignedTo.ToString("dd MMM yyyy"),
                        AssignedFromRaw = a.AssignedFrom,
                        AssignedToRaw = a.AssignedTo
                    })
                    .ToListAsync();

                return Json(new { success = true, data = assignments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEditVehicleAssignment(int id = 0)
        {
            ViewBag.Routes = await _context.TblTrnRoutes
                .Where(r => r.IsActive)
                .OrderBy(r => r.RouteName)
                .ToListAsync();

            ViewBag.Vehicles = await _context.TblTrnVehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();

            ViewBag.Drivers = await _context.TblTrnDrivers
                .Where(d => d.IsActive)
                .OrderBy(d => d.DriverName)
                .ToListAsync();

            ViewBag.Conductors = await _context.TblTrnConductors
                .Where(c => c.IsActive)
                .OrderBy(c => c.ConductorName)
                .ToListAsync();

            var model = id == 0
                ? new TblTrnVehicleAssignment
                  {
                      IsActive = true,
                      AssignedFrom = DateOnly.FromDateTime(DateTime.Today),
                      AssignedTo = DateOnly.FromDateTime(DateTime.Today.AddMonths(6))
                  }
                : await _context.TblTrnVehicleAssignments.FindAsync(id)
                  ?? new TblTrnVehicleAssignment { IsActive = true };

            return PartialView("_VehicleAssignmentForm", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateOrEditVehicleAssignment(int id, TblTrnVehicleAssignment model)
        {
            try
            {
                // Date overlap check: same vehicle on overlapping dates
                var overlapQuery = _context.TblTrnVehicleAssignments
                    .Where(a => a.VehicleId == model.VehicleId
                             && a.IsActive
                             && a.AssignmentId != id
                             && a.AssignedFrom <= model.AssignedTo
                             && a.AssignedTo >= model.AssignedFrom);

                if (await overlapQuery.AnyAsync())
                    return Json(new { success = false, message = "Vehicle already has an overlapping assignment in the selected date range." });

                if (id == 0)
                {
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnVehicleAssignments.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Vehicle assignment created successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnVehicleAssignments.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Assignment not found." });

                    existing.RouteId = model.RouteId;
                    existing.VehicleId = model.VehicleId;
                    existing.DriverId = model.DriverId;
                    existing.ConductorId = model.ConductorId;
                    existing.AssignedFrom = model.AssignedFrom;
                    existing.AssignedTo = model.AssignedTo;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Vehicle assignment updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteVehicleAssignment(int id)
        {
            try
            {
                var assignment = await _context.TblTrnVehicleAssignments.FindAsync(id);
                if (assignment == null)
                    return Json(new { success = false, message = "Assignment not found." });

                assignment.IsActive = false;
                assignment.UpdatedBy = CurrentUserId;
                assignment.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Vehicle assignment deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── STUDENT ASSIGNMENTS ───────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetStudentAssignments(int? sessionId, int? routeId)
        {
            try
            {
                var query = _context.TblTrnStudentAssignments
                    .Where(sa => sa.IsActive)
                    .Include(sa => sa.Student)
                    .Include(sa => sa.Route)
                    .Include(sa => sa.Stop)
                    .Include(sa => sa.Session)
                    .AsQueryable();

                if (sessionId.HasValue && sessionId > 0)
                    query = query.Where(sa => sa.SessionId == sessionId);

                if (routeId.HasValue && routeId > 0)
                    query = query.Where(sa => sa.RouteId == routeId);

                var assignments = await query
                    .OrderBy(sa => sa.Route.RouteName)
                    .ThenBy(sa => sa.Student.StudentName)
                    .Select(sa => new
                    {
                        sa.Id,
                        StudentName = sa.Student.StudentName,
                        sa.Student.AdmissionNo,
                        RouteName = sa.Route.RouteName,
                        StopName = sa.Stop.StopName,
                        StopOrder = sa.Stop.StopOrder,
                        FareAmount = sa.Stop.FareAmount,
                        SessionName = sa.Session.SessionName,
                        sa.AssignmentStatus
                    })
                    .ToListAsync();

                return Json(new { success = true, data = assignments });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetAvailableStudents(int sessionId, int routeId)
        {
            try
            {
                // Students in this session not already assigned to this route
                var assignedStudentIds = await _context.TblTrnStudentAssignments
                    .Where(sa => sa.RouteId == routeId && sa.SessionId == sessionId && sa.IsActive)
                    .Select(sa => sa.StudentId)
                    .ToListAsync();

                var students = await _context.TblStudentSessions
                    .Where(ss => ss.SessionId == sessionId && ss.IsActive == true
                              && ss.StudentId.HasValue
                              && !assignedStudentIds.Contains(ss.StudentId!.Value))
                    .Include(ss => ss.Student)
                    .Include(ss => ss.Class)
                    .OrderBy(ss => ss.Student!.StudentName)
                    .Select(ss => new
                    {
                        StudentId = ss.StudentId!.Value,
                        StudentName = ss.Student != null ? ss.Student.StudentName : "-",
                        AdmissionNo = ss.Student != null ? ss.Student.AdmissionNo : "-",
                        ClassName = ss.Class != null ? ss.Class.ClassName : "-"
                    })
                    .ToListAsync();

                return Json(new { success = true, data = students });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AssignStudents([FromBody] BulkAssignDto dto)
        {
            try
            {
                if (dto.StudentIds == null || !dto.StudentIds.Any())
                    return Json(new { success = false, message = "No students selected." });

                // Capacity check
                var route = await _context.TblTrnRoutes.FindAsync(dto.RouteId);
                if (route == null)
                    return Json(new { success = false, message = "Route not found." });

                var currentCount = await _context.TblTrnStudentAssignments
                    .CountAsync(sa => sa.RouteId == dto.RouteId && sa.SessionId == dto.SessionId
                                   && sa.IsActive && sa.AssignmentStatus == "Active");

                if (currentCount + dto.StudentIds.Count > route.MaxStudentCapacity)
                    return Json(new { success = false, message = $"Capacity exceeded. Route allows {route.MaxStudentCapacity} students, currently {currentCount} assigned." });

                var stop = await _context.TblTrnRouteStops.FindAsync(dto.StopId);
                if (stop == null)
                    return Json(new { success = false, message = "Stop not found." });

                // Auto fee generation
                var feeType = await _context.TblFeeTypes
                    .FirstOrDefaultAsync(f => f.FeeCategory == "Transport" && f.IsActive == true);
                if (feeType == null)
                {
                    feeType = new TblFeeType
                    {
                        FeeName = "Transport Fee",
                        FeeCategory = "Transport",
                        IsActive = true,
                        CreatedDate = DateTime.Now
                    };
                    _context.TblFeeTypes.Add(feeType);
                    await _context.SaveChangesAsync();
                }

                int assigned = 0;
                foreach (var studentId in dto.StudentIds)
                {
                    // Skip if already assigned
                    var alreadyAssigned = await _context.TblTrnStudentAssignments
                        .AnyAsync(sa => sa.StudentId == studentId && sa.RouteId == dto.RouteId
                                     && sa.SessionId == dto.SessionId && sa.IsActive);
                    if (alreadyAssigned) continue;

                    // Create optional fee
                    var existingFee = await _context.TblStudentOptionalFees
                        .FirstOrDefaultAsync(f => f.StudentId == studentId
                                                && f.SessionId == dto.SessionId
                                                && f.FeeTypeId == feeType.FeeTypeId
                                                && f.IsActive);

                    int optionalFeeId;
                    if (existingFee != null)
                    {
                        existingFee.Amount = stop.FareAmount;
                        existingFee.UpdatedBy = CurrentUserId;
                        existingFee.UpdatedDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                        optionalFeeId = existingFee.Id;
                    }
                    else
                    {
                        var newFee = new TblStudentOptionalFee
                        {
                            StudentId = studentId,
                            SessionId = dto.SessionId,
                            FeeTypeId = feeType.FeeTypeId,
                            Amount = stop.FareAmount,
                            IsActive = true,
                            CreatedBy = CurrentUserId,
                            CreatedDate = DateTime.Now
                        };
                        _context.TblStudentOptionalFees.Add(newFee);
                        await _context.SaveChangesAsync();
                        optionalFeeId = newFee.Id;
                    }

                    // Create student assignment
                    _context.TblTrnStudentAssignments.Add(new TblTrnStudentAssignment
                    {
                        StudentId = studentId,
                        RouteId = dto.RouteId,
                        StopId = dto.StopId,
                        SessionId = dto.SessionId,
                        AssignmentStatus = "Active",
                        OptionalFeeId = optionalFeeId,
                        IsActive = true,
                        CreatedBy = CurrentUserId,
                        CreatedDate = DateTime.Now
                    });
                    assigned++;
                }
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{assigned} student(s) assigned to route successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> ChangeStudentStop(int assignmentId, int newStopId)
        {
            try
            {
                var assignment = await _context.TblTrnStudentAssignments.FindAsync(assignmentId);
                if (assignment == null)
                    return Json(new { success = false, message = "Assignment not found." });

                var newStop = await _context.TblTrnRouteStops.FindAsync(newStopId);
                if (newStop == null)
                    return Json(new { success = false, message = "Stop not found." });

                assignment.StopId = newStopId;
                assignment.UpdatedBy = CurrentUserId;
                assignment.UpdatedDate = DateTime.Now;

                // Recalculate fee
                if (assignment.OptionalFeeId.HasValue)
                {
                    var fee = await _context.TblStudentOptionalFees
                        .FindAsync(assignment.OptionalFeeId.Value);
                    if (fee != null)
                    {
                        fee.Amount = newStop.FareAmount;
                        fee.UpdatedBy = CurrentUserId;
                        fee.UpdatedDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student stop changed successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeactivateStudentAssignment(int id)
        {
            try
            {
                var assignment = await _context.TblTrnStudentAssignments.FindAsync(id);
                if (assignment == null)
                    return Json(new { success = false, message = "Assignment not found." });

                assignment.IsActive = false;
                assignment.AssignmentStatus = "Inactive";
                assignment.UpdatedBy = CurrentUserId;
                assignment.UpdatedDate = DateTime.Now;

                // Deactivate associated fee
                if (assignment.OptionalFeeId.HasValue)
                {
                    var fee = await _context.TblStudentOptionalFees
                        .FindAsync(assignment.OptionalFeeId.Value);
                    if (fee != null)
                    {
                        fee.IsActive = false;
                        fee.UpdatedBy = CurrentUserId;
                        fee.UpdatedDate = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Student assignment deactivated." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    public class BulkAssignDto
    {
        public int RouteId { get; set; }
        public int StopId { get; set; }
        public int SessionId { get; set; }
        public List<int> StudentIds { get; set; } = new();
    }
}
