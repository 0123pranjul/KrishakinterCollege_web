using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportDriverController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportDriverController(LibmanagementContext context)
            => _context = context;

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 1;

        public IActionResult Index() => View();

        // ── DRIVERS ──────────────────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetAllDrivers()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var warnDate = today.AddDays(30);

                var drivers = await _context.TblTrnDrivers
                    .Where(d => d.IsActive)
                    .OrderBy(d => d.DriverName)
                    .Select(d => new
                    {
                        d.DriverId,
                        d.DriverName,
                        d.ContactNumber,
                        d.LicenseNumber,
                        LicenseExpiry = d.LicenseExpiry.HasValue ? d.LicenseExpiry.Value.ToString("dd MMM yyyy") : "-",
                        LicenseExpiryRaw = d.LicenseExpiry,
                        d.Address,
                        LicenseWarn = d.LicenseExpiry.HasValue && d.LicenseExpiry.Value <= warnDate && d.LicenseExpiry.Value >= today,
                        LicenseExpired = d.LicenseExpiry.HasValue && d.LicenseExpiry.Value < today
                    })
                    .ToListAsync();

                return Json(new { success = true, data = drivers });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEditDriver(int id = 0)
        {
            var model = id == 0
                ? new TblTrnDriver { IsActive = true }
                : await _context.TblTrnDrivers.FindAsync(id)
                  ?? new TblTrnDriver { IsActive = true };

            return PartialView("_DriverForm", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateOrEditDriver(int id, TblTrnDriver model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnDrivers.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Driver added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnDrivers.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Driver not found." });

                    existing.DriverName = model.DriverName;
                    existing.ContactNumber = model.ContactNumber;
                    existing.LicenseNumber = model.LicenseNumber;
                    existing.LicenseExpiry = model.LicenseExpiry;
                    existing.EmployeeId = model.EmployeeId;
                    existing.PhotoUrl = model.PhotoUrl;
                    existing.Address = model.Address;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Driver updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteDriver(int id)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var hasActiveAssignment = await _context.TblTrnVehicleAssignments
                    .AnyAsync(a => a.DriverId == id && a.IsActive && a.AssignedTo >= today);
                if (hasActiveAssignment)
                    return Json(new { success = false, message = "Cannot delete driver: they have an active route assignment." });

                var driver = await _context.TblTrnDrivers.FindAsync(id);
                if (driver == null)
                    return Json(new { success = false, message = "Driver not found." });

                driver.IsActive = false;
                driver.UpdatedBy = CurrentUserId;
                driver.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Driver deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── CONDUCTORS ───────────────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetAllConductors()
        {
            try
            {
                var conductors = await _context.TblTrnConductors
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.ConductorName)
                    .Select(c => new
                    {
                        c.ConductorId,
                        c.ConductorName,
                        c.ContactNumber,
                        c.EmployeeId
                    })
                    .ToListAsync();

                return Json(new { success = true, data = conductors });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEditConductor(int id = 0)
        {
            var model = id == 0
                ? new TblTrnConductor { IsActive = true }
                : await _context.TblTrnConductors.FindAsync(id)
                  ?? new TblTrnConductor { IsActive = true };

            return PartialView("_ConductorForm", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateOrEditConductor(int id, TblTrnConductor model)
        {
            try
            {
                if (id == 0)
                {
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnConductors.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Conductor added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnConductors.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Conductor not found." });

                    existing.ConductorName = model.ConductorName;
                    existing.ContactNumber = model.ContactNumber;
                    existing.EmployeeId = model.EmployeeId;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Conductor updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteConductor(int id)
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var hasActiveAssignment = await _context.TblTrnVehicleAssignments
                    .AnyAsync(a => a.ConductorId == id && a.IsActive && a.AssignedTo >= today);
                if (hasActiveAssignment)
                    return Json(new { success = false, message = "Cannot delete conductor: they have an active route assignment." });

                var conductor = await _context.TblTrnConductors.FindAsync(id);
                if (conductor == null)
                    return Json(new { success = false, message = "Conductor not found." });

                conductor.IsActive = false;
                conductor.UpdatedBy = CurrentUserId;
                conductor.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Conductor deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
