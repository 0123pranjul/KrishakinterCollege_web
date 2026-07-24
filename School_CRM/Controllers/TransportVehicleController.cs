using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;

namespace School_CRM.Controllers
{
    [Authorize]
    public class TransportVehicleController : Controller
    {
        private readonly LibmanagementContext _context;

        public TransportVehicleController(LibmanagementContext context)
            => _context = context;

        private int CurrentUserId =>
            int.TryParse(HttpContext.Request.Cookies["EmployeeId"], out var id) ? id : 1;

        public IActionResult Index() => View();

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var warnDate = today.AddDays(30);

                var vehicles = await _context.TblTrnVehicles
                    .Where(v => v.IsActive)
                    .OrderBy(v => v.RegistrationNumber)
                    .Select(v => new
                    {
                        v.VehicleId,
                        v.RegistrationNumber,
                        v.VehicleType,
                        v.Make,
                        v.Model,
                        v.ManufactureYear,
                        v.MaxCapacity,
                        InsuranceExpiry = v.InsuranceExpiry.HasValue ? v.InsuranceExpiry.Value.ToString("dd MMM yyyy") : "-",
                        InsuranceExpiryRaw = v.InsuranceExpiry,
                        FitnessExpiry = v.FitnessExpiry.HasValue ? v.FitnessExpiry.Value.ToString("dd MMM yyyy") : "-",
                        FitnessExpiryRaw = v.FitnessExpiry,
                        v.Remarks,
                        InsuranceWarn = v.InsuranceExpiry.HasValue && v.InsuranceExpiry.Value <= warnDate && v.InsuranceExpiry.Value >= today,
                        InsuranceExpired = v.InsuranceExpiry.HasValue && v.InsuranceExpiry.Value < today,
                        FitnessWarn = v.FitnessExpiry.HasValue && v.FitnessExpiry.Value <= warnDate && v.FitnessExpiry.Value >= today,
                        FitnessExpired = v.FitnessExpiry.HasValue && v.FitnessExpiry.Value < today
                    })
                    .ToListAsync();

                return Json(new { success = true, data = vehicles });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int id = 0)
        {
            var model = id == 0
                ? new TblTrnVehicle { IsActive = true, MaxCapacity = 40 }
                : await _context.TblTrnVehicles.FindAsync(id)
                  ?? new TblTrnVehicle { IsActive = true, MaxCapacity = 40 };

            return PartialView("_VehicleForm", model);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateOrEdit(int id, TblTrnVehicle model)
        {
            try
            {
                // Duplicate registration check
                var dupExists = await _context.TblTrnVehicles
                    .AnyAsync(v => v.RegistrationNumber == model.RegistrationNumber
                               && v.IsActive
                               && v.VehicleId != id);
                if (dupExists)
                    return Json(new { success = false, message = $"Vehicle with registration '{model.RegistrationNumber}' already exists." });

                if (id == 0)
                {
                    model.CreatedBy = CurrentUserId;
                    model.CreatedDate = DateTime.Now;
                    model.IsActive = true;
                    _context.TblTrnVehicles.Add(model);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Vehicle added successfully." });
                }
                else
                {
                    var existing = await _context.TblTrnVehicles.FindAsync(id);
                    if (existing == null)
                        return Json(new { success = false, message = "Vehicle not found." });

                    existing.RegistrationNumber = model.RegistrationNumber;
                    existing.VehicleType = model.VehicleType;
                    existing.Make = model.Make;
                    existing.Model = model.Model;
                    existing.ManufactureYear = model.ManufactureYear;
                    existing.MaxCapacity = model.MaxCapacity;
                    existing.InsuranceExpiry = model.InsuranceExpiry;
                    existing.FitnessExpiry = model.FitnessExpiry;
                    existing.PhotoUrl = model.PhotoUrl;
                    existing.Remarks = model.Remarks;
                    existing.UpdatedBy = CurrentUserId;
                    existing.UpdatedDate = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Json(new { success = true, message = "Vehicle updated successfully." });
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
                var today = DateOnly.FromDateTime(DateTime.Today);
                var hasActiveAssignment = await _context.TblTrnVehicleAssignments
                    .AnyAsync(a => a.VehicleId == id && a.IsActive && a.AssignedTo >= today);
                if (hasActiveAssignment)
                    return Json(new { success = false, message = "Cannot delete vehicle: it has an active route assignment." });

                var vehicle = await _context.TblTrnVehicles.FindAsync(id);
                if (vehicle == null)
                    return Json(new { success = false, message = "Vehicle not found." });

                vehicle.IsActive = false;
                vehicle.UpdatedBy = CurrentUserId;
                vehicle.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Vehicle deleted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
