using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Assets/Issue/[action]")]
    public class AssetIssueController : Controller
    {
        private readonly IAssetIssueService   _issueSvc;
        private readonly IAssetPersonService  _personSvc;
        private readonly IAssetLocationService _locSvc;

        public AssetIssueController(
            IAssetIssueService issueSvc,
            IAssetPersonService personSvc,
            IAssetLocationService locSvc)
        {
            _issueSvc  = issueSvc;
            _personSvc = personSvc;
            _locSvc    = locSvc;
        }

        // ── ISSUE ─────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Create() =>
            View(new IssueAssetDto { IssueDate = DateOnly.FromDateTime(DateTime.Today) });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueAssetDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            dto.IssuedBy = UserId();
            var (ok, msg, issueId) = await _issueSvc.IssueAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(IssueSlip), new { id = issueId }); }
            TempData["Error"] = msg;
            return View(dto);
        }

        // ── RETURN ────────────────────────────────────────────────────────
        [HttpGet]
        public IActionResult Return() => View(new ReturnAssetDto());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnAssetDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            dto.ReturnedTo = UserId();
            var (ok, msg) = await _issueSvc.ReturnAsync(dto);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Return));
        }

        // ── MOVE ──────────────────────────────────────────────────────────
        [HttpGet("{unitId}")]
        public async Task<IActionResult> Move(int unitId)
        {
            var locations = await _locSvc.GetAllAsync();
            ViewBag.Locations = locations;
            return View(new MoveAssetDto { UnitId = unitId, MoveDate = DateTime.Now });
        }

        [HttpPost("{unitId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Move(int unitId, MoveAssetDto dto)
        {
            if (unitId != dto.UnitId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Locations = await _locSvc.GetAllAsync();
                return View(dto);
            }
            dto.MovedBy = UserId();
            var (ok, msg) = await _issueSvc.MoveAssetAsync(dto);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction("Units", "Asset", new { id = 0 });
        }

        // ── OVERDUE ───────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> Overdue() =>
            View(await _issueSvc.GetOverdueAsync());

        // ── ISSUE SLIP ────────────────────────────────────────────────────
        [HttpGet("{id}")]
        public IActionResult IssueSlip(int id)
        {
            ViewBag.IssueId = id;
            return View();
        }

        // ── AJAX ──────────────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> CheckAsset(string assetTag)
        {
            var result = await _issueSvc.CheckAssetEligibilityAsync(assetTag);
            return Json(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetReturnInfo(string assetTag)
        {
            var info = await _issueSvc.GetReturnInfoAsync(assetTag);
            if (info == null) return Json(new { success = false, message = "No open issue found." });
            return Json(new { success = true, data = info });
        }

        [HttpGet]
        public async Task<IActionResult> GetPersonList(string type, string? search = null)
        {
            var list = await _personSvc.GetPersonListAsync(type, search);
            return Json(list);
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
