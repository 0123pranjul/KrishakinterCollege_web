using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Store/Product/[action]")]
    public class StoreProductController : Controller
    {
        private readonly IInvProductService  _productSvc;
        private readonly IInvCategoryService _catSvc;
        private readonly IInvUnitService     _unitSvc;
        private readonly IInvStockAdjustmentService _adjSvc;

        public StoreProductController(
            IInvProductService productSvc,
            IInvCategoryService catSvc,
            IInvUnitService unitSvc,
            IInvStockAdjustmentService adjSvc)
        {
            _productSvc = productSvc;
            _catSvc     = catSvc;
            _unitSvc    = unitSvc;
            _adjSvc     = adjSvc;
        }

        [HttpGet]
        public async Task<IActionResult> Index(InvProductSearchDto filter)
        {
            var (items, total) = await _productSvc.SearchAsync(filter);
            ViewBag.Categories  = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName");
            ViewBag.TotalCount  = total;
            ViewBag.CurrentPage = filter.PageNumber;
            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)filter.PageSize);
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new InvProductDto { ReorderLevel = 5, GSTPercent = 0 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvProductDto dto)
        {
            if (!ModelState.IsValid) { await LoadDropdowns(dto.CategoryId); return View(dto); }
            var (ok, msg, productId) = await _productSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Index)); }
            TempData["Error"] = msg;
            await LoadDropdowns(dto.CategoryId);
            return View(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var p = await _productSvc.GetByIdAsync(id);
            if (p == null) return NotFound();
            await LoadDropdowns(p.CategoryId);
            return View(new InvProductDto
            {
                ProductId    = p.ProductId,
                ProductCode  = p.ProductCode,
                ProductName  = p.ProductName,
                CategoryId   = p.CategoryId,
                UnitId       = p.UnitId,
                CostPrice    = p.CostPrice,
                SellingPrice = p.SellingPrice,
                ReorderLevel = p.ReorderLevel,
                MaxStockLevel = p.MaxStockLevel,
                Description  = p.Description,
                HSNCode      = p.Hsncode,
                GSTPercent   = p.Gstpercent,
                Barcode      = p.Barcode,
                IsActive     = p.IsActive,
                CurrentStock = p.CurrentStock,
                ProductImagePath = p.ProductImagePath
            });
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, InvProductDto dto)
        {
            if (id != dto.ProductId) return BadRequest();
            if (!ModelState.IsValid) { await LoadDropdowns(dto.CategoryId); return View(dto); }
            var (ok, msg) = await _productSvc.UpdateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Index)); }
            TempData["Error"] = msg;
            await LoadDropdowns(dto.CategoryId);
            return View(dto);
        }

        // Stock Adjustment
        [HttpGet("{id}")]
        public async Task<IActionResult> Adjust(int id)
        {
            var p = await _productSvc.GetByIdAsync(id);
            if (p == null) return NotFound();
            return View(new InvStockAdjustmentDto
            {
                ProductId    = p.ProductId,
                ProductName  = p.ProductName,
                CurrentStock = p.CurrentStock
            });
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adjust(int id, InvStockAdjustmentDto dto)
        {
            if (id != dto.ProductId) return BadRequest();
            if (!ModelState.IsValid) return View(dto);
            var (ok, msg) = await _adjSvc.AdjustAsync(dto, UserId());
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }

        // AJAX: product search for sale/PO forms
        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<object>());
            var results = await _productSvc.SearchLookupAsync(q);
            return Json(results);
        }

        // AJAX: get product by id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _productSvc.GetByIdAsync(id);
            if (p == null) return Json(new { success = false });
            return Json(new
            {
                success      = true,
                productId    = p.ProductId,
                productName  = p.ProductName,
                costPrice    = p.CostPrice,
                sellingPrice = p.SellingPrice,
                currentStock = p.CurrentStock,
                unitShort    = p.Unit.UnitShort,
                gstPercent   = p.Gstpercent
            });
        }

        private async Task LoadDropdowns(int? catId = null)
        {
            ViewBag.Categories = new SelectList(await _catSvc.GetAllAsync(), "CategoryId", "CategoryName", catId);
            ViewBag.Units      = new SelectList(await _unitSvc.GetAllAsync(), "UnitId", "UnitName");
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
