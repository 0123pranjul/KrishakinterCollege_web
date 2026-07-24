using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using School_CRM.Services.Interface;

namespace School_CRM.Controllers
{
    [Authorize]
    [Route("Assets/Dashboard/[action]")]
    public class AssetDashboardController : Controller
    {
        private readonly IAssetDashboardService _dashSvc;

        public AssetDashboardController(IAssetDashboardService dashSvc)
        {
            _dashSvc = dashSvc;
        }

        [HttpGet]
        [Route("/Assets/Dashboard")]
        public async Task<IActionResult> Index()
        {
            var roleName = Request.Cookies["roleName"] ?? "";
            if (IsAdmin(roleName))
                return RedirectToAction(nameof(Admin));
            return RedirectToAction(nameof(Member));
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
            var roleName = Request.Cookies["roleName"] ?? "";
            var userType = roleName.ToLower() == "student" ? "Student" : "Teacher";
            var entityId = int.TryParse(Request.Cookies["EntityId"], out var id) ? id : 0;

            var dashboard = await _dashSvc.GetMemberDashboardAsync(userType, entityId);
            ViewBag.UserType = userType;
            return View(dashboard);
        }

        private static bool IsAdmin(string role) =>
            role.ToLower() is "superadmin" or "admin" or "staff";
    }
}
