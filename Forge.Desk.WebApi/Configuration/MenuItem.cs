namespace Forge.Desk.WebApi.Configuration
{
    public class MenuItem
    {
        public string MenuName { get; set; } = string.Empty;
        public MenuItemType Type { get; set; }
        public string? Icon { get; set; }
        public string? Controller { get; set; }
        public string? Action { get; set; }
        public string? Route { get; set; }
        public List<MenuItem> Children { get; set; } = new();
    }
}
