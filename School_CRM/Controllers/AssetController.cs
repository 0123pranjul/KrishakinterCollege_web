using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    public class AssetController : Controller
    {
        private readonly IAssetMasterService     _assetSvc;
        private readonly IAssetCategoryService   _catSvc;
        private readonly IAssetSubCategoryService _subSvc;
        private readonly IAssetVendorService     _vendorSvc;
        private readonly IAssetLocationService   _locSvc;

        public AssetController(
            IAssetMasterService assetSvc,
            IAssetCategoryService catSvc,
            IAssetSubCategoryService subSvc,
            IAssetVendorService vendorSvc,
            IAssetLocationService locSvc)
        {
            _assetSvc  = assetSvc;
            _catSvc    = catSvc;
            _subSvc    = subSvc;
            _vendorSvc = vendorSvc;
            _locSvc    = locSvc;
        }

        // ── LIST ──────────────────────────────────────────────────────────
        [HttpGet("/Assets")]
        [HttpGet("/Assets/Index")]
        public async Task<IActionResult> Index(AssetSearchDto filter)
        {
            var (items, total) = await _assetSvc.SearchAsync(filter);
            ViewBag.Categories  = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
            ViewBag.TotalCount  = total;
            ViewBag.CurrentPage = filter.PageNumber;
            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)filter.PageSize);
            return View(items);
        }

        // ── CREATE ────────────────────────────────────────────────────────
        [HttpGet("/Assets/Create")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new AssetMasterDto { IsIssuable = true, NumberOfUnits = 1 });
        }

        [HttpPost("/Assets/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssetMasterDto dto)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(dto.CategoryId); return View(dto); }

            // Resolve category name for code generation
            var cat = await _catSvc.GetByIdAsync(dto.CategoryId);
            dto.CategoryName = cat?.CategoryName;

            var (ok, msg, assetId) = await _assetSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Units), new { id = assetId }); }
            TempData["Error"] = msg;
            await LoadDropdowns(dto.CategoryId);
            return View(dto);
        }

        // ── EDIT ──────────────────────────────────────────────────────────
        [HttpGet("/Assets/Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var asset = await _assetSvc.GetByIdAsync(id);
            if (asset == null) return NotFound();
            await LoadDropdowns(asset.CategoryId);
            return View(new AssetMasterDto
            {
                AssetId        = asset.AssetId,
                AssetCode      = asset.AssetCode,
                AssetName      = asset.AssetName,
                CategoryId     = asset.CategoryId,
                SubCategoryId  = asset.SubCategoryId,
                Brand          = asset.Brand,
                Model          = asset.Model,
                Specifications = asset.Specifications,
                UnitPrice      = asset.UnitPrice,
                IsIssuable     = asset.IsIssuable,
                AssetImagePath = asset.AssetImagePath,
                IsActive       = asset.IsActive
            });
        }

        [HttpPost("/Assets/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AssetMasterDto dto)
        {
            if (id != dto.AssetId) return BadRequest();
            if (!ModelState.IsValid) { await LoadDropdowns(dto.CategoryId); return View(dto); }
            var (ok, msg) = await _assetSvc.UpdateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Index)); }
            TempData["Error"] = msg;
            await LoadDropdowns(dto.CategoryId);
            return View(dto);
        }

        // ── UNITS / QR ────────────────────────────────────────────────────
        [HttpGet("/Assets/Units/{id}")]
        public async Task<IActionResult> Units(int id)
        {
            var asset = await _assetSvc.GetByIdAsync(id);
            if (asset == null) return NotFound();
            var units = await _assetSvc.GetUnitsAsync(id);
            ViewBag.Asset = asset;
            return View(units);
        }

        [HttpGet("/Assets/AddUnits/{id}")]
        public async Task<IActionResult> AddUnits(int id)
        {
            var asset = await _assetSvc.GetByIdAsync(id);
            if (asset == null) return NotFound();
            ViewBag.Vendors   = new SelectList(await _vendorSvc.GetAllAsync(), "VendorId", "VendorName");
            ViewBag.Locations = new SelectList(await _locSvc.GetAllAsync(), "LocationId", "LocationName");
            return View(new AddUnitsDto
            {
                AssetId       = asset.AssetId,
                AssetName     = asset.AssetName,
                PurchasePrice = asset.UnitPrice
            });
        }

        [HttpPost("/Assets/AddUnits/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddUnits(int id, AddUnitsDto dto)
        {
            if (id != dto.AssetId) return BadRequest();
            if (!ModelState.IsValid)
            {
                ViewBag.Vendors   = new SelectList(await _vendorSvc.GetAllAsync(), "VendorId", "VendorName");
                ViewBag.Locations = new SelectList(await _locSvc.GetAllAsync(), "LocationId", "LocationName");
                return View(dto);
            }
            var (ok, msg) = await _assetSvc.AddUnitsAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Units), new { id = dto.AssetId }); }
            TempData["Error"] = msg;
            return View(dto);
        }

        // ── QR IMAGE ──────────────────────────────────────────────────────
        [HttpGet("/Assets/QR/{assetTag}")]
        [AllowAnonymous]
        public async Task<IActionResult> QRImage(string assetTag)
        {
            var bytes = await _assetSvc.GetQRImageAsync(assetTag);
            if (bytes == null) return NotFound();
            return File(bytes, "image/png");
        }

        // ── SCAN PAGE ─────────────────────────────────────────────────────
        [HttpGet("/Assets/Scan/{assetTag}")]
        [AllowAnonymous]
        public async Task<IActionResult> Scan(string assetTag)
        {
            var info = await _assetSvc.GetScanInfoAsync(assetTag);
            if (info == null) return NotFound("Asset not found.");
            return View(info);
        }

        // ── AJAX ──────────────────────────────────────────────────────────
        [HttpGet("/Assets/GetUnitByTag/{assetTag}")]
        public async Task<IActionResult> GetUnitByTag(string assetTag)
        {
            var info = await _assetSvc.GetScanInfoAsync(assetTag);
            if (info == null) return Json(new { success = false, message = "Asset tag not found." });
            return Json(new { success = true, data = info });
        }

        // ── HELPERS ───────────────────────────────────────────────────────
        private async Task LoadDropdowns(int? selectedCategoryId = null)
        {
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName", selectedCategoryId);
            ViewBag.Vendors    = new SelectList(await _vendorSvc.GetAllAsync(), "VendorId", "VendorName");
            ViewBag.Locations  = new SelectList(await _locSvc.GetAllAsync(), "LocationId", "LocationName");
            if (selectedCategoryId.HasValue)
                ViewBag.SubCategories = new SelectList(
                    await _subSvc.GetByCategoryAsync(selectedCategoryId.Value), "SubCategoryId", "SubCategoryName");
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
