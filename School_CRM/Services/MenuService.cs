using School_CRM.Services;
using Microsoft.EntityFrameworkCore;
using School_CRM.Models.ViewModels;
using School_CRM.Models;
using School_CRM.Services.Interface;

namespace School_CRM.Services
{
    public class MenuService : IMenuService
    {
        private readonly LibmanagementContext _db;
        public MenuService(LibmanagementContext db) { _db = db; }

        public async Task<List<MenuItemViewModel>> GetMenusByRoleAsync(string roleId)
        {
            // Get all menu permissions for this role where CanRead = true, ordered by DisplayOrder
            var menus = await _db.TblMenuPermissions
                .Where(mp => mp.RoleId == Convert.ToInt32(roleId) && mp.CanRead)
                .Select(mp => mp.Menu)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync();

            // Recursive function to build hierarchy
            List<MenuItemViewModel> BuildHierarchy(int? parentId)
            {
                return menus
                    .Where(m => m.ParentId == parentId)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => new MenuItemViewModel
                    {
                        MenuId = m.MenuId,
                        MenuName = m.MenuName,
                        ControllerName = m.ControllerName,
                        ActionName = m.ActionName,
                        Url = m.Url,
                        Icon = m.Icon,
                        Children = BuildHierarchy(m.MenuId) // recursion
                    })
                    .ToList();
            }

            // Start with top-level menus (ParentId == null)
            var parentMenus = BuildHierarchy(null);

            return parentMenus;
        }
    }
}
