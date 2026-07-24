using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Store/PurchaseOrder/[action]")]
    public class StorePurchaseController : Controller
    {
        private readonly IInvPurchaseOrderService _poSvc;
        private readonly IInvSupplierService      _supplierSvc;
        private readonly IInvProductService       _productSvc;

        public StorePurchaseController(
            IInvPurchaseOrderService poSvc,
            IInvSupplierService supplierSvc,
            IInvProductService productSvc)
        {
            _poSvc       = poSvc;
            _supplierSvc = supplierSvc;
            _productSvc  = productSvc;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? supplierId, string? status)
        {
            ViewBag.Suppliers = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
            return View(await _poSvc.GetAllAsync(supplierId, status));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
            return View(new InvPurchaseOrderDto
            {
                OrderDate = DateOnly.FromDateTime(DateTime.Today),
                Items     = new List<InvPOItemDto> { new() }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvPurchaseOrderDto dto)
        {
            // Remove empty rows
            dto.Items = dto.Items.Where(i => i.ProductId > 0 && i.OrderQty > 0).ToList();

            if (!dto.Items.Any())
                ModelState.AddModelError("", "At least one item is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
                return View(dto);
            }

            var (ok, msg, poId) = await _poSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Details), new { id = poId }); }
            TempData["Error"] = msg;
            ViewBag.Suppliers = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
            return View(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var po = await _poSvc.GetByIdAsync(id);
            if (po == null) return NotFound();
            return View(po);
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Send(int id)
        {
            var (ok, msg) = await _poSvc.SendToSupplierAsync(id, UserId());
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var (ok, msg) = await _poSvc.CancelAsync(id);
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(Index));
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
