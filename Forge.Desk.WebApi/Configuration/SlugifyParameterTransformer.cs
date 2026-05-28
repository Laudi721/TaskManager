using System.Text.RegularExpressions;

namespace Forge.Desk.WebApi.Configuration
{
    /// <summary>
    /// Converts PascalCase controller names into kebab-case route segments
    /// (UserSettings → user-settings, Roles → roles).
    /// </summary>
    public sealed class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        private static readonly Regex SlugRegex = new(
            "([a-z0-9])([A-Z])",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public string? TransformOutbound(object? value)
        {
            if (value is null)
            {
                return null;
            }

            var name = value.ToString();
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            return SlugRegex.Replace(name, "$1-$2").ToLowerInvariant();
        }
    }
}
