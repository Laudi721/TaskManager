using System.Xml.Linq;
using Forge.Application.Common;
using Forge.Common.Dtos;
using Microsoft.Extensions.Options;

namespace Forge.Desk.WebApi.Configuration
{
    public class XmlMenuProvider : IMenuProvider
    {
        private readonly IHostEnvironment _environment;
        private readonly MenuOptions _options;
        private readonly Lazy<IReadOnlyList<MenuItem>> _menu;

        public XmlMenuProvider(IHostEnvironment environment, IOptions<MenuOptions> options)
        {
            _environment = environment;
            _options = options.Value;
            _menu = new Lazy<IReadOnlyList<MenuItem>>(Load);
        }

        public IReadOnlyList<MenuItem> GetMenu() => _menu.Value;

        private IReadOnlyList<MenuItem> Load()
        {
            var path = Path.IsPathRooted(_options.FilePath)
                ? _options.FilePath
                : Path.Combine(_environment.ContentRootPath, _options.FilePath);

            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Menu definition file was not found.", path);
            }

            var document = XDocument.Load(path);
            var menuRoot = document.Root?.Element("Menu");

            if (menuRoot == null)
            {
                return Array.Empty<MenuItem>();
            }

            return menuRoot.Elements("MenuItem").Select(Parse).ToList();
        }

        private static MenuItem Parse(XElement element)
        {
            var item = new MenuItem
            {
                MenuName = element.Attribute("menuName")?.Value ?? string.Empty,
                Type = ParseType(element.Attribute("type")?.Value),
                Icon = element.Attribute("icon")?.Value,
                Controller = element.Attribute("controller")?.Value,
                Action = element.Attribute("action")?.Value,
                Route = element.Attribute("route")?.Value,
                Children = element.Elements("MenuItem").Select(Parse).ToList()
            };

            return item;
        }

        private static MenuItemType ParseType(string? value)
        {
            return Enum.TryParse<MenuItemType>(value, ignoreCase: true, out var parsed)
                ? parsed
                : MenuItemType.Group;
        }
    }
}
