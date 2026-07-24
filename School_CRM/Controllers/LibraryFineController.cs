using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Library/Fine")]
    public class LibraryFineController : Controller
    {
        private readonly IFinePaymentService _fineService;
        private readonly IFinePolicyService _policyService;

        public LibraryFineController(IFinePaymentService fineService, IFinePolicyService policyService)
        {
            _fineService  = fineService;
            _policyService = policyService;
        }

        // ============================================================
        // FINE PAYMENT FORM
        // ============================================================
        [HttpGet("Pay/{issueId}")]
        public async Task<IActionResult> Pay(int issueId)
        {
            var details = await _fineService.GetFineDetailsAsync(issueId);
            if (details == null)
                return NotFound("Issue transaction not found.");

            return View(details);
        }

        [HttpPost("Pay/{issueId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(int issueId, FinePaymentDto dto)
        {
            if (issueId != dto.IssueId)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                var details = await _fineService.GetFineDetailsAsync(issueId);
                return View(details);
            }

            dto.CollectedBy = GetCurrentUserId();
            var (success, message, receiptNo) = await _fineService.CollectFineAsync(dto);

            if (success)
            {
                TempData["Success"] = message;
                TempData["ReceiptNo"] = receiptNo;
                return RedirectToAction(nameof(Receipt), new { receiptNo });
            }

            TempData["Error"] = message;
            var fineDetails = await _fineService.GetFineDetailsAsync(issueId);
            return View(fineDetails);
        }

        // ============================================================
        // FINE RECEIPT
        // ============================================================
        [HttpGet("Receipt/{receiptNo}")]
        public IActionResult Receipt(string receiptNo)
        {
            ViewBag.ReceiptNo = receiptNo;
            return View();
        }

        // ============================================================
        // FINE POLICY MANAGEMENT
        // ============================================================
        [HttpGet("Policy")]
        public async Task<IActionResult> Policy()
        {
            var policies = await _policyService.GetAllPoliciesAsync();
            return View(policies);
        }

        [HttpGet("Policy/Create")]
        public IActionResult CreatePolicy()
        {
            return View(new FinePolicyDto
            {
                PerDayFine          = 1.00m,
                GracePeriodDays     = 0,
                MaxBooksForStudent  = 2,
                MaxBooksForTeacher  = 5,
                IssueDaysForStudent = 14,
                IssueDaysForTeacher = 30,
                DamageFineType      = "Percentage",
                DamageFineValue     = 50.00m,
                LostFineType        = "BookPrice",
                LostFineValue       = 1.00m
            });
        }

        [HttpPost("Policy/Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePolicy(FinePolicyDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userId = GetCurrentUserId();
            var (success, message) = await _policyService.CreatePolicyAsync(dto, userId);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Policy));
            }

            TempData["Error"] = message;
            return View(dto);
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 1;
        }
    }
}
