using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Store/Sale/[action]")]
    public class StoreSaleController : Controller
    {
        private readonly IInvSaleService    _saleSvc;
        private readonly IInvPersonService  _personSvc;

        public StoreSaleController(IInvSaleService saleSvc, IInvPersonService personSvc)
        {
            _saleSvc   = saleSvc;
            _personSvc = personSvc;
        }

        [HttpGet]
        public async Task<IActionResult> Index(InvSaleFilterDto filter)
        {
            var (items, total) = await _saleSvc.GetAllAsync(filter);
            ViewBag.TotalCount  = total;
            ViewBag.CurrentPage = filter.PageNumber;
            ViewBag.TotalPages  = (int)Math.Ceiling(total / (double)filter.PageSize);
            return View(items);
        }

        [HttpGet]
        public IActionResult Create() =>
            View(new InvSaleDto
            {
                BillType     = "Sale",
                CustomerType = "Student",
                SaleDate     = DateOnly.FromDateTime(DateTime.Today),
                PaymentMode  = "Cash",
                Items        = new List<InvSaleItemDto> { new() }
            });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvSaleDto dto)
        {
            dto.Items = dto.Items.Where(i => i.ProductId > 0 && i.Qty > 0).ToList();

            if (!dto.Items.Any())
                ModelState.AddModelError("", "At least one item is required.");

            if (dto.CustomerType == "Walk-in" && string.IsNullOrWhiteSpace(dto.CustomerName))
                ModelState.AddModelError("CustomerName", "Customer name is required for Walk-in.");

            if (!ModelState.IsValid) return View(dto);

            var (ok, msg, saleId) = await _saleSvc.CreateAsync(dto, UserId());
            if (ok) { TempData["Success"] = msg; return RedirectToAction(nameof(Bill), new { id = saleId }); }
            TempData["Error"] = msg;
            return View(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var sale = await _saleSvc.GetByIdAsync(id);
            if (sale == null) return NotFound();
            return View(sale);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Bill(int id)
        {
            var sale = await _saleSvc.GetByIdAsync(id);
            if (sale == null) return NotFound();
            return View(sale);
        }

        // Credit payment
        [HttpGet]
        public async Task<IActionResult> CreditPending()
        {
            var (items, _) = await _saleSvc.GetAllAsync(new InvSaleFilterDto { PageSize = 1000 });
            var unpaid = items.Where(x => !x.IsPaid && x.BillType == "Sale").ToList();
            return View(unpaid);
        }

        [HttpGet("{type}/{id}")]
        public async Task<IActionResult> CollectPayment(string type, int id)
        {
            var details = await _saleSvc.GetCreditDetailsAsync(type, id);
            if (details == null) return NotFound();

            // Resolve customer name
            var person = await _personSvc.GetPersonAsync(type, id);
            if (details != null) details.CustomerName = person?.Name ?? $"{type} #{id}";

            return View(details);
        }

        [HttpPost("{type}/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CollectPayment(string type, int id, InvCreditPaymentDto dto)
        {
            if (!ModelState.IsValid)
            {
                var details = await _saleSvc.GetCreditDetailsAsync(type, id);
                return View(details);
            }

            var (ok, msg) = await _saleSvc.CollectPaymentAsync(dto, UserId());
            TempData[ok ? "Success" : "Error"] = msg;
            return RedirectToAction(nameof(CreditPending));
        }

        // AJAX: get person list
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
