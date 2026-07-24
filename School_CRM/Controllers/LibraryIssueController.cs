using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Models.DTOs;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Library/Issue")]
    public class LibraryIssueController : Controller
    {
        private readonly IIssueService _issueService;
        private readonly ILibraryMemberService _memberService;
        private readonly IFinePolicyService _policyService;

        public LibraryIssueController(
            IIssueService issueService,
            ILibraryMemberService memberService,
            IFinePolicyService policyService)
        {
            _issueService  = issueService;
            _memberService = memberService;
            _policyService = policyService;
        }

        // ============================================================
        // ISSUE BOOK FORM
        // ============================================================
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var policy = await _policyService.GetActivePolicyAsync();
            ViewBag.Policy = policy;
            return View(new IssueBookDto { IssueDate = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IssueBookDto dto)
        {
            if (!ModelState.IsValid)
            {
                var policy = await _policyService.GetActivePolicyAsync();
                ViewBag.Policy = policy;
                return View(dto);
            }

            dto.IssuedBy = GetCurrentUserId();
            var (success, message, issueId) = await _issueService.IssueBookAsync(dto);

            if (success)
            {
                TempData["Success"] = message;
                TempData["IssueId"] = issueId;
                return RedirectToAction(nameof(IssueSlip), new { id = issueId });
            }

            TempData["Error"] = message;
            var pol = await _policyService.GetActivePolicyAsync();
            ViewBag.Policy = pol;
            return View(dto);
        }

        // ============================================================
        // RETURN BOOK FORM
        // ============================================================
        [HttpGet("Return")]
        public IActionResult Return()
        {
            return View(new ReturnBookDto());
        }

        [HttpPost("Return")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(ReturnBookDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            dto.ReturnedTo = GetCurrentUserId();
            var (success, message, fineAmount) = await _issueService.ReturnBookAsync(dto);

            if (success)
            {
                TempData["Success"] = message;
                TempData["FineAmount"] = fineAmount;
                return RedirectToAction(nameof(Return));
            }

            TempData["Error"] = message;
            return View(dto);
        }

        // ============================================================
        // OVERDUE LIST
        // ============================================================
        [HttpGet("Overdue")]
        public async Task<IActionResult> Overdue()
        {
            var overdueList = await _issueService.GetOverdueBooksAsync();
            return View(overdueList);
        }

        // ============================================================
        // MEMBER HISTORY
        // ============================================================
        [HttpGet("MemberHistory")]
        public async Task<IActionResult> MemberHistory(string userType, int userId, int page = 1)
        {
            var member = await _memberService.GetMemberAsync(userType, userId);
            if (member == null)
                return NotFound();

            var history = await _issueService.GetMemberHistoryAsync(userType, userId, page);
            ViewBag.Member = member;
            ViewBag.Page   = page;
            return View(history);
        }

        // ============================================================
        // MARK BOOK AS LOST
        // ============================================================
        [HttpGet("MarkLost/{issueId}")]
        public IActionResult MarkLost(int issueId)
        {
            return View(new MarkLostDto
            {
                IssueId  = issueId,
                LostDate = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        [HttpPost("MarkLost/{issueId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkLost(int issueId, MarkLostDto dto)
        {
            if (issueId != dto.IssueId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(dto);

            var staffId = GetCurrentUserId();
            var (success, message) = await _issueService.MarkBookLostAsync(dto, staffId);

            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Overdue));
        }

        // ============================================================
        // ISSUE SLIP
        // ============================================================
        [HttpGet("IssueSlip/{id}")]
        public IActionResult IssueSlip(int id)
        {
            ViewBag.IssueId = id;
            return View();
        }

        // ============================================================
        // AJAX ENDPOINTS
        // ============================================================

        /// <summary>
        /// AJAX: Get members by type (Student/Teacher)
        /// </summary>
        [HttpGet("GetMembers")]
        public async Task<IActionResult> GetMembers(string userType, string? search = null)
        {
            if (userType == "Student")
            {
                var students = await _memberService.GetStudentsAsync(search);
                return Json(students);
            }
            else if (userType == "Teacher")
            {
                var teachers = await _memberService.GetTeachersAsync(search);
                return Json(teachers);
            }

            return Json(new List<object>());
        }

        /// <summary>
        /// AJAX: Check member eligibility
        /// </summary>
        [HttpGet("CheckEligibility")]
        public async Task<IActionResult> CheckEligibility(string userType, int userId)
        {
            var result = await _issueService.CheckMemberEligibilityAsync(userType, userId);
            return Json(result);
        }

        /// <summary>
        /// AJAX: Get book info by accession number
        /// </summary>
        [HttpGet("GetBookInfo")]
        public async Task<IActionResult> GetBookInfo(string accessionNo)
        {
            var policy = await _policyService.GetActivePolicyAsync();
            if (policy == null)
                return Json(new { success = false, message = "No active policy found." });

            // Get return info (which includes book details)
            var info = await _issueService.GetReturnInfoByAccessionAsync(accessionNo);

            // For issue form, we need copy info
            // Return basic info for display
            return Json(new { success = info != null, data = info });
        }

        /// <summary>
        /// AJAX: Get return info by accession number
        /// </summary>
        [HttpGet("GetReturnInfo")]
        public async Task<IActionResult> GetReturnInfo(string accessionNo)
        {
            var info = await _issueService.GetReturnInfoByAccessionAsync(accessionNo);
            if (info == null)
                return Json(new { success = false, message = "No open issue found for this accession number." });

            return Json(new { success = true, data = info });
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
