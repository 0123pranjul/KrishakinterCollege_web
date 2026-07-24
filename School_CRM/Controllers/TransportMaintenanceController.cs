using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportMaintenanceController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportMaintenanceController(LibmanagementContext context)
            => _context = context;

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 1;

        public async Task<IActionResult> Index()
        {
            ViewBag.Vehicles = await _context.TblTrnVehicles
                .Where(v => v.IsActive)
                .OrderBy(v => v.RegistrationNumber)
                .ToListAsync();
            return View();
        }

        // ── MAINTENANCE ───────────────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetMaintenance(int? vehicleId)
        {
            try
            {
                var query = _context.TblTrnMaintenanceLogs
                    .Where(m => m.IsActive)
                    .Include(m => m.Vehicle)
                    .AsQueryable();

                if (vehicleId.HasValue && vehicleId > 0)
                    query = query.Where(m => m.VehicleId == vehicleId);

                var today = DateOnly.FromDateTime(DateTime.Today);
                var logs = await query
                    .OrderByDescending(m => m.ServiceDate)
                    .Select(m => new
                    {
                        m.Id,
                        VehicleNo = m.Vehicle.RegistrationNumber,
                        m.VehicleId,
                        m.ServiceType,
                        ServiceDate = m.ServiceDate.ToString("dd MMM yyyy"),
                        m.ServiceCost,
                        m.ServiceProvider,
                        NextDue = m.NextServiceDueDate.HasValue ? m.NextServiceDueDate.Value.ToString("dd MMM yyyy") : "-",
                        NextDueRaw = m.NextServiceDueDate,
                        IsOverdue = m.NextServiceDueDate.HasValue && m.NextServiceDueDate.Value < today,
                        m.Remarks
                    })
                    .ToListAsync();

                return Json(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveMaintenance([FromBody] TblTrnMaintenanceLog model)
        {
            try
            {
                if (model.Id == 0)
                {
                    model.IsActive = true;
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    _context.TblTrnMaintenanceLogs.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Maintenance log added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnMaintenanceLogs.FindAsync(model.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Maintenance log not found." });

                    existing.VehicleId = model.VehicleId;
                    existing.ServiceType = model.ServiceType;
                    existing.ServiceDate = model.ServiceDate;
                    existing.ServiceCost = model.ServiceCost;
                    existing.ServiceProvider = model.ServiceProvider;
                    existing.NextServiceDueDate = model.NextServiceDueDate;
                    existing.Remarks = model.Remarks;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Maintenance log updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── FUEL LOG ──────────────────────────────────────────────────────────

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetFuelLog(int? vehicleId)
        {
            try
            {
                var query = _context.TblTrnFuelLogs
                    .Where(f => f.IsActive)
                    .Include(f => f.Vehicle)
                    .AsQueryable();

                if (vehicleId.HasValue && vehicleId > 0)
                    query = query.Where(f => f.VehicleId == vehicleId);

                var logs = await query
                    .OrderByDescending(f => f.FuelDate)
                    .Select(f => new
                    {
                        f.Id,
                        VehicleNo = f.Vehicle.RegistrationNumber,
                        f.VehicleId,
                        FuelDate = f.FuelDate.ToString("dd MMM yyyy"),
                        f.FuelQuantityLitres,
                        f.FuelCostPerLitre,
                        TotalCost = f.TotalFuelCost ?? (f.FuelQuantityLitres * f.FuelCostPerLitre),
                        f.OdometerReading,
                        f.FuelStation
                    })
                    .ToListAsync();

                return Json(new { success = true, data = logs });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveFuelLog([FromBody] TblTrnFuelLog model)
        {
            try
            {
                if (model.Id == 0)
                {
                    // Odometer validation: new reading should be >= last reading for same vehicle
                    var lastReading = await _context.TblTrnFuelLogs
                        .Where(f => f.VehicleId == model.VehicleId && f.IsActive)
                        .OrderByDescending(f => f.FuelDate)
                        .ThenByDescending(f => f.Id)
                        .Select(f => f.OdometerReading)
                        .FirstOrDefaultAsync();

                    if (lastReading > 0 && model.OdometerReading < lastReading)
                        return Json(new { success = false, message = $"Odometer reading ({model.OdometerReading}) cannot be less than the last recorded reading ({lastReading})." });

                    model.IsActive = true;
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    _context.TblTrnFuelLogs.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Fuel log added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnFuelLogs.FindAsync(model.Id);
                    if (existing == null)
                        return Json(new { success = false, message = "Fuel log not found." });

                    existing.VehicleId = model.VehicleId;
                    existing.FuelDate = model.FuelDate;
                    existing.FuelQuantityLitres = model.FuelQuantityLitres;
                    existing.FuelCostPerLitre = model.FuelCostPerLitre;
                    existing.OdometerReading = model.OdometerReading;
                    existing.FuelStation = model.FuelStation;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Fuel log updated successfully." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
