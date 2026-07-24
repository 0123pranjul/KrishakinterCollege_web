using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Store/StockReceipt/[action]")]
    public class StoreGRNController : Controller
    {
        private readonly IInvStockReceiptService  _grnSvc;
        private readonly IInvSupplierService      _supplierSvc;
        private readonly IInvPurchaseOrderService _poSvc;

        public StoreGRNController(
            IInvStockReceiptService grnSvc,
            IInvSupplierService supplierSvc,
            IInvPurchaseOrderService poSvc)
        {
            _grnSvc      = grnSvc;
            _supplierSvc = supplierSvc;
            _poSvc       = poSvc;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? supplierId)
        {
            ViewBag.Suppliers = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
            return View(await _grnSvc.GetAllAsync(supplierId));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ViewBag.Suppliers = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
            ViewBag.PendingPOs = new SelectList(await _poSvc.GetPendingAsync(), "Poid", "Ponumber");
            return View(new InvStockReceiptDto
            {
                ReceiptDate = DateOnly.FromDateTime(DateTime.Today),
                Items       = new List<InvGRNItemDto> { new() }
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvStockReceiptDto dto)
        {
            dto.Items = dto.Items.Where(i => i.ProductId > 0 && i.ReceivedQty > 0).ToList();

            if (!dto.Items.Any())
                ModelState.AddModelError("", "At least one item is required.");

            if (!ModelState.IsValid)
            {
                ViewBag.Suppliers  = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
                ViewBag.PendingPOs = new SelectList(await _poSvc.GetPendingAsync(), "Poid", "Ponumber");
                return View(dto);
            }

            var (ok, msg, receiptId) = await _grnSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Details), new { id = receiptId }); }
            TempData["Error"] = msg;
            ViewBag.Suppliers  = new SelectList(await _supplierSvc.GetAllAsync(), "SupplierId", "SupplierName");
            ViewBag.PendingPOs = new SelectList(await _poSvc.GetPendingAsync(), "Poid", "Ponumber");
            return View(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _grnSvc.GetByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        // AJAX: load PO items when PO is selected
        [HttpGet("{poId}")]
        public async Task<IActionResult> GetPOItems(int poId)
        {
            var items = await _grnSvc.GetPOItemsAsync(poId);
            return Json(items);
        }

        private int UserId()
        {
            var v = User.FindFirst("UserId")?.Value;
            return int.TryParse(v, out var id) ? id : 1;
        }
    }
}
