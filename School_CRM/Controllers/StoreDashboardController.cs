using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Store/Dashboard/[action]")]
    public class StoreDashboardController : Controller
    {
        private readonly IInvDashboardService _dashSvc;

        public StoreDashboardController(IInvDashboardService dashSvc)
        {
            _dashSvc = dashSvc;
        }

        [HttpGet]
        [Route("/Store/Dashboard")]
        public IActionResult Index()
        {
            var role = Request.Cookies["roleName"] ?? "";
            return IsAdmin(role)
                ? RedirectToAction(nameof(Admin))
                : RedirectToAction(nameof(Member));
        }

        [HttpGet]
        public async Task<IActionResult> Admin()
        {
            var dashboard = await _dashSvc.GetAdminDashboardAsync();
            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> Member()
        {
            var role     = Request.Cookies["roleName"] ?? "";
            var userType = role.ToLower() == "student" ? "Student" : "Teacher";
            var entityId = int.TryParse(Request.Cookies["EntityId"], out var id) ? id : 0;

            var dashboard = await _dashSvc.GetMemberDashboardAsync(userType, entityId);
            ViewBag.UserType = userType;
            return View(dashboard);
        }

        private static bool IsAdmin(string role) =>
            role.ToLower() is "superadmin" or "admin" or "storekeeper" or "staff";
    }
}
