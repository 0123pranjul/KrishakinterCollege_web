using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Assets/Maintenance/[action]")]
    public class AssetMaintenanceController : Controller
    {
        private readonly IAssetMaintenanceService  _maintSvc;
        private readonly IAssetDamageReportService _damageSvc;
        private readonly IAssetDisposalService     _disposalSvc;
        private readonly IAssetVendorService       _vendorSvc;

        public AssetMaintenanceController(
            IAssetMaintenanceService maintSvc,
            IAssetDamageReportService damageSvc,
            IAssetDisposalService disposalSvc,
            IAssetVendorService vendorSvc)
        {
            _maintSvc    = maintSvc;
            _damageSvc   = damageSvc;
            _disposalSvc = disposalSvc;
            _vendorSvc   = vendorSvc;
        }

        // ── MAINTENANCE ───────────────────────────────────────────────────
        [HttpGet("{unitId}")]
        public async Task<IActionResult> Create(int unitId)
        {
            ViewBag.Vendors = new SelectList(await _vendorSvc.GetAllAsync(), "VendorId", "VendorName");
            return View(new MaintenanceLogDto
            {
                UnitId    = unitId,
                StartDate = DateOnly.FromDateTime(DateTime.Today),
                Status    = "Pending"
            });
        }

        [HttpPost("{unitId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int unitId, MaintenanceLogDto dto)
        {
            if (unitId != dto.UnitId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Vendors = new SelectList(await _vendorSvc.GetAllAsync(), "VendorId", "VendorName");
                return View(dto);
            }
            dto.CreatedBy = UserId();
            var (ok, msg) = await _maintSvc.CreateAsync(dto);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction("Units", "Asset", new { id = 0 });
        }

        [HttpGet("{unitId}")]
        public async Task<IActionResult> History(int unitId) =>
            View(await _maintSvc.GetByUnitAsync(unitId));

        // ── DAMAGE / LOSS ─────────────────────────────────────────────────
        [HttpGet("{unitId}")]
        public IActionResult DamageReport(int unitId) =>
            View(new DamageLossReportDto
            {
                UnitId     = unitId,
                ReportDate = DateOnly.FromDateTime(DateTime.Today),
                ReportedBy = UserId()
            });

        [HttpPost("{unitId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DamageReport(int unitId, DamageLossReportDto dto)
        {
            if (unitId != dto.UnitId) return BadRequest();
            if (!ModelState.IsValid) return View(dto);
            dto.ReportedBy = UserId();
            var (ok, msg) = await _damageSvc.CreateAsync(dto);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction("Index", "AssetDashboard");
        }

        // ── DISPOSAL ──────────────────────────────────────────────────────
        [HttpGet("{unitId}")]
        public IActionResult Dispose(int unitId) =>
            View(new DisposalDto
            {
                UnitId      = unitId,
                DisposalDate = DateOnly.FromDateTime(DateTime.Today),
                AuthorizedBy = UserId(),
                CreatedBy    = UserId()
            });

        [HttpPost("{unitId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispose(int unitId, DisposalDto dto)
        {
            if (unitId != dto.UnitId) return BadRequest();
            if (!ModelState.IsValid) return View(dto);
            dto.AuthorizedBy = UserId();
            dto.CreatedBy    = UserId();
            var (ok, msg) = await _disposalSvc.DisposeAsync(dto);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction("Index", "AssetDashboard");
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
