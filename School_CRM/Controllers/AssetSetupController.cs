using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    /// <summary>
    /// Handles Category, SubCategory, Location, Vendor CRUD.
    /// Route prefix: /Assets/Setup/...
    /// </summary>
    [Authorize]
    [Route("Assets/[action]")]
    public class AssetSetupController : Controller
    {
        private readonly IAssetCategoryService    _catSvc;
        private readonly IAssetSubCategoryService _subSvc;
        private readonly IAssetLocationService    _locSvc;
        private readonly IAssetVendorService      _vendorSvc;

        public AssetSetupController(
            IAssetCategoryService catSvc,
            IAssetSubCategoryService subSvc,
            IAssetLocationService locSvc,
            IAssetVendorService vendorSvc)
        {
            _catSvc    = catSvc;
            _subSvc    = subSvc;
            _locSvc    = locSvc;
            _vendorSvc = vendorSvc;
        }

        // ── CATEGORIES ────────────────────────────────────────────────────
        [HttpGet("/Assets/Category")]
        public async Task<IActionResult> Categories() =>
            View(await _catSvc.GetAllAsync(false));

        [HttpGet("/Assets/Category/Create")]
        public IActionResult CategoryCreate() =>
            View(new AssetCategoryDto { IsActive = true });

        [HttpPost("/Assets/Category/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(AssetCategoryDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _catSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Categories)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpGet("/Assets/Category/Edit/{id}")]
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var entity = await _catSvc.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return View(new AssetCategoryDto
            {
                CategoryId   = entity.CategoryId,
                CategoryName = entity.CategoryName,
                Description  = entity.Description,
                IsActive     = entity.IsActive
            });
        }

        [HttpPost("/Assets/Category/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(AssetCategoryDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _catSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Categories)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpPost("/Assets/Category/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var (ok, msg) = await _catSvc.DeleteAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Categories));
        }

        // ── SUB-CATEGORIES ────────────────────────────────────────────────
        [HttpGet("/Assets/SubCategory")]
        public async Task<IActionResult> SubCategories()
        {
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
            return View(await _subSvc.GetAllAsync(false));
        }

        [HttpGet("/Assets/SubCategory/Create")]
        public async Task<IActionResult> SubCategoryCreate()
        {
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
            return View(new AssetSubCategoryDto { IsActive = true });
        }

        [HttpPost("/Assets/SubCategory/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubCategoryCreate(AssetSubCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
                return View(dto);
            }
            var (ok, msg) = await _subSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(SubCategories)); }
            ModelState.AddModelError("", msg);
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
            return View(dto);
        }

        [HttpGet("/Assets/SubCategory/Edit/{id}")]
        public async Task<IActionResult> SubCategoryEdit(int id)
        {
            var entity = await _subSvc.GetByIdAsync(id);
            if (entity == null) return NotFound();
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName", entity.CategoryId);
            return View(new AssetSubCategoryDto
            {
                SubCategoryId   = entity.SubCategoryId,
                CategoryId      = entity.CategoryId,
                SubCategoryName = entity.SubCategoryName,
                Description     = entity.Description,
                IsActive        = entity.IsActive
            });
        }

        [HttpPost("/Assets/SubCategory/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubCategoryEdit(AssetSubCategoryDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
                return View(dto);
            }
            var (ok, msg) = await _subSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(SubCategories)); }
            ModelState.AddModelError("", msg);
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
            return View(dto);
        }

        // ── LOCATIONS ─────────────────────────────────────────────────────
        [HttpGet("/Assets/Location")]
        public async Task<IActionResult> Locations() =>
            View(await _locSvc.GetAllAsync(false));

        [HttpGet("/Assets/Location/Create")]
        public IActionResult LocationCreate() =>
            View(new AssetLocationDto { IsActive = true });

        [HttpPost("/Assets/Location/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LocationCreate(AssetLocationDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _locSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Locations)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpGet("/Assets/Location/Edit/{id}")]
        public async Task<IActionResult> LocationEdit(int id)
        {
            var entity = await _locSvc.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return View(new AssetLocationDto
            {
                LocationId   = entity.LocationId,
                LocationName = entity.LocationName,
                LocationType = entity.LocationType,
                Floor        = entity.Floor,
                Building     = entity.Building,
                IsActive     = entity.IsActive
            });
        }

        [HttpPost("/Assets/Location/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LocationEdit(AssetLocationDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _locSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Locations)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        // ── VENDORS ───────────────────────────────────────────────────────
        [HttpGet("/Assets/Vendor")]
        public async Task<IActionResult> Vendors() =>
            View(await _vendorSvc.GetAllAsync(false));

        [HttpGet("/Assets/Vendor/Create")]
        public IActionResult VendorCreate() =>
            View(new AssetVendorDto { IsActive = true });

        [HttpPost("/Assets/Vendor/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VendorCreate(AssetVendorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _vendorSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Vendors)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpGet("/Assets/Vendor/Edit/{id}")]
        public async Task<IActionResult> VendorEdit(int id)
        {
            var entity = await _vendorSvc.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return View(new AssetVendorDto
            {
                VendorId      = entity.VendorId,
                VendorName    = entity.VendorName,
                ContactPerson = entity.ContactPerson,
                Phone         = entity.Phone,
                Email         = entity.Email,
                Address       = entity.Address,
                GSTNo         = entity.Gstno,
                IsActive      = entity.IsActive
            });
        }

        [HttpPost("/Assets/Vendor/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VendorEdit(AssetVendorDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _vendorSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Vendors)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        // ── AJAX ──────────────────────────────────────────────────────────
        [HttpGet("/Assets/GetSubCategories/{categoryId}")]
        public async Task<IActionResult> GetSubCategories(int categoryId)
        {
            var list = await _subSvc.GetByCategoryAsync(categoryId);
            return Json(list.Select(x => new { x.SubCategoryId, x.SubCategoryName }));
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
