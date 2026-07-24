using School_CRM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models;


namespace LibManagement.Controllers
{
    public class MenuController : Controller
    {
        private readonly LibmanagementContext _context;

        public MenuController(LibmanagementContext context)
        {
            _context = context;
        }

        // ── Page ──────────────────────────────────────────────────────────
        public IActionResult Index() => View();

        // ── GET: All menus ────────────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var menus = await _context.TblMenus
                .OrderBy(m => m.DisplayOrder)
                .Select(m => new
                {
                    m.MenuId,
                    m.ParentId,
                    m.MenuName,
                    m.ControllerName,
                    m.ActionName,
                    Url = m.Url,
                    Icon = m.Icon,
                    m.DisplayOrder,
                    m.IsActive
                })
                .ToListAsync();

            return Json(menus);
        }

        // ── GET: Single menu by ID ────────────────────────────────────────
        [HttpGet]
        public async Task<IActionResult> GetById(int id)
        {
            var menu = await _context.TblMenus.FindAsync(id);
            if (menu == null)
                return Json(new { error = "Menu not found!" });

            return Json(new
            {
                menu.MenuId,
                menu.ParentId,
                menu.MenuName,
                menu.ControllerName,
                menu.ActionName,
                Url = menu.Url,
                Icon = menu.Icon,
                menu.DisplayOrder,
                menu.IsActive
            });
        }

        // ── POST: Save (Insert or Update) ────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Save([FromBody] MenuSaveDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.MenuName))
                return Json(new { success = false, message = "Menu name is required!" });

            if (dto.DisplayOrder < 1)
                return Json(new { success = false, message = "Display order must be ≥ 1!" });

            // Prevent self-referencing parent
            if (dto.MenuId > 0 && dto.ParentId == dto.MenuId)
                return Json(new { success = false, message = "A menu cannot be its own parent!" });

            if (dto.MenuId == 0)
            {
                // INSERT
                var menu = new TblMenu
                {
                    ParentId = dto.ParentId,
                    MenuName = dto.MenuName.Trim(),
                    ControllerName = dto.ControllerName?.Trim(),
                    ActionName = dto.ActionName?.Trim(),
                    Url = dto.Url?.Trim(),
                    Icon = dto.Icon?.Trim(),
                    DisplayOrder = dto.DisplayOrder,
                    IsActive = dto.IsActive
                };
                _context.TblMenus.Add(menu);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Menu added successfully!", menuId = menu.MenuId });
            }
            else
            {
                // UPDATE
                var menu = await _context.TblMenus.FindAsync(dto.MenuId);
                if (menu == null)
                    return Json(new { success = false, message = "Menu not found!" });

                menu.ParentId = dto.ParentId;
                menu.MenuName = dto.MenuName.Trim();
                menu.ControllerName = dto.ControllerName?.Trim();
                menu.ActionName = dto.ActionName?.Trim();
                menu.Url = dto.Url?.Trim();
                menu.Icon = dto.Icon?.Trim();
                menu.DisplayOrder = dto.DisplayOrder;
                menu.IsActive = dto.IsActive;

                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Menu updated successfully!" });
            }
        }

        // ── POST: Delete ──────────────────────────────────────────────────
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var menu = await _context.TblMenus.FindAsync(id);
            if (menu == null)
                return Json(new { success = false, message = "Menu not found!" });

            // Check for child menus
            bool hasChildren = await _context.TblMenus.AnyAsync(m => m.ParentId == id);
            if (hasChildren)
                return Json(new { success = false, message = "Cannot delete! This menu has sub-menus. Delete sub-menus first." });

            _context.TblMenus.Remove(menu);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Menu deleted successfully!" });
        }
    }

    // ── DTO ───────────────────────────────────────────────────────────────
    public class MenuSaveDto
    {
        public int MenuId { get; set; }
        public int? ParentId { get; set; }
        public string MenuName { get; set; } = string.Empty;
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public int DisplayOrder { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }
}
