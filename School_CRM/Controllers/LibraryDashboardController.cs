using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Library/Dashboard")]
    public class LibraryDashboardController : Controller
    {
        private readonly ILibraryDashboardService _dashboardService;

        public LibraryDashboardController(ILibraryDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // ============================================================
        // LIBRARIAN / ADMIN DASHBOARD
        // ============================================================
        [HttpGet("")]
        [HttpGet("Librarian")]
        public async Task<IActionResult> Librarian()
        {
            var roleName = Request.Cookies["roleName"] ?? "";
            
            // Only SuperAdmin, Admin, Librarian can access this
            if (!IsLibrarianOrAdmin(roleName))
                return RedirectToAction(nameof(Member));

            var dashboard = await _dashboardService.GetLibrarianDashboardAsync();
            return View(dashboard);
        }

        // ============================================================
        // MEMBER DASHBOARD (Student / Teacher)
        // ============================================================
        [HttpGet("Member")]
        public async Task<IActionResult> Member()
        {
            var roleName = Request.Cookies["roleName"] ?? "";
            var userType = GetUserType(roleName);
            var userId   = GetEntityId();

            var dashboard = await _dashboardService.GetMemberDashboardAsync(userType, userId);
            ViewBag.UserType = userType;
            ViewBag.UserId   = userId;
            return View(dashboard);
        }

        // ============================================================
        // HELPERS
        // ============================================================
        private bool IsLibrarianOrAdmin(string roleName)
        {
            var role = roleName.ToLower();
            return role is "superadmin" or "admin" or "librarian" or "principal";
        }

        private string GetUserType(string roleName)
        {
            return roleName.ToLower() == "student" ? "Student" : "Teacher";
        }

        private int GetEntityId()
        {
            var entityId = Request.Cookies["EntityId"];
            return int.TryParse(entityId, out var id) ? id : 0;
        }
    }
}
