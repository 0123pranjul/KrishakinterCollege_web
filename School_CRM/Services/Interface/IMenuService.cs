using School_CRM.Models.ViewModels;

namespace School_CRM.Services.Interface
{
    public interface IMenuService
    {
        Task<List<MenuItemViewModel>> GetMenusByRoleAsync(string roleId);
    }
}
