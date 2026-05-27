namespace Forge.Common.Dtos
{
    /// <summary>
    /// Per-user preferences persisted in the backend. Currently only theme; intentionally a
    /// dedicated DTO so further preferences (language, density, etc.) can be added without
    /// breaking the auth payload.
    /// </summary>
    public class UserPreferencesDto
    {
        public string? ThemePreference { get; set; }
    }
}
