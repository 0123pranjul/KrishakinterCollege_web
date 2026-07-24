namespace School_CRM.Models.ViewModels
{
    public class MenuItemViewModel
    {
        public int MenuId { get; set; }
        public string MenuName { get; set; }
        public string? ControllerName { get; set; }
        public string? ActionName { get; set; }
        public string? Url { get; set; }
        public string? Icon { get; set; }
        public List<MenuItemViewModel> Children { get; set; } = new List<MenuItemViewModel>();
    }
}
