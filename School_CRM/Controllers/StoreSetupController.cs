using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Store/[action]")]
    public class StoreSetupController : Controller
    {
        private readonly IInvCategoryService _catSvc;
        private readonly IInvUnitService     _unitSvc;
        private readonly IInvSupplierService _supplierSvc;

        public StoreSetupController(
            IInvCategoryService catSvc,
            IInvUnitService unitSvc,
            IInvSupplierService supplierSvc)
        {
            _catSvc      = catSvc;
            _unitSvc     = unitSvc;
            _supplierSvc = supplierSvc;
        }

        // ── CATEGORIES ────────────────────────────────────────────────────
        [HttpGet("/Store/Category")]
        public async Task<IActionResult> Categories() =>
            View(await _catSvc.GetAllAsync(false));

        [HttpGet("/Store/Category/Create")]
        public IActionResult CategoryCreate() =>
            View(new InvCategoryDto { IsActive = true });

        [HttpPost("/Store/Category/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryCreate(InvCategoryDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _catSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Categories)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpGet("/Store/Category/Edit/{id}")]
        public async Task<IActionResult> CategoryEdit(int id)
        {
            var e = await _catSvc.GetByIdAsync(id);
            if (e == null) return NotFound();
            return View(new InvCategoryDto { CategoryId = e.CategoryId, CategoryName = e.CategoryName, Description = e.Description, IsActive = e.IsActive });
        }

        [HttpPost("/Store/Category/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryEdit(InvCategoryDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _catSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Categories)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpPost("/Store/Category/Delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CategoryDelete(int id)
        {
            var (ok, msg) = await _catSvc.DeleteAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Categories));
        }

        // ── UNITS ─────────────────────────────────────────────────────────
        [HttpGet("/Store/Unit")]
        public async Task<IActionResult> Units() =>
            View(await _unitSvc.GetAllAsync(false));

        [HttpGet("/Store/Unit/Create")]
        public IActionResult UnitCreate() =>
            View(new InvUnitDto { IsActive = true });

        [HttpPost("/Store/Unit/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnitCreate(InvUnitDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _unitSvc.CreateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Units)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpGet("/Store/Unit/Edit/{id}")]
        public async Task<IActionResult> UnitEdit(int id)
        {
            var e = await _unitSvc.GetByIdAsync(id);
            if (e == null) return NotFound();
            return View(new InvUnitDto { UnitId = e.UnitId, UnitName = e.UnitName, UnitShort = e.UnitShort, IsActive = e.IsActive });
        }

        [HttpPost("/Store/Unit/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnitEdit(InvUnitDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _unitSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Units)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        // ── SUPPLIERS ─────────────────────────────────────────────────────
        [HttpGet("/Store/Supplier")]
        public async Task<IActionResult> Suppliers() =>
            View(await _supplierSvc.GetAllAsync(false));

        [HttpGet("/Store/Supplier/Create")]
        public IActionResult SupplierCreate() =>
            View(new InvSupplierDto { IsActive = true });

        [HttpPost("/Store/Supplier/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplierCreate(InvSupplierDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _supplierSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Suppliers)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        [HttpGet("/Store/Supplier/Edit/{id}")]
        public async Task<IActionResult> SupplierEdit(int id)
        {
            var e = await _supplierSvc.GetByIdAsync(id);
            if (e == null) return NotFound();
            return View(new InvSupplierDto
            {
                SupplierId = e.SupplierId, SupplierName = e.SupplierName,
                ContactPerson = e.ContactPerson, Phone = e.Phone, Email = e.Email,
                Address = e.Address, GSTNo = e.Gstno, OpeningBalance = e.OpeningBalance, IsActive = e.IsActive
            });
        }

        [HttpPost("/Store/Supplier/Edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SupplierEdit(InvSupplierDto dto)
        {
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _supplierSvc.UpdateAsync(dto);
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Suppliers)); }
            ModelState.AddModelError("", msg);
            return View(dto);
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
