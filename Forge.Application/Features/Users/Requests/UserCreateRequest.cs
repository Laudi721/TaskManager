namespace Forge.Application.Features.Users.Requests
{
    public class UserCreateRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public IReadOnlyList<int> RoleIds { get; set; } = Array.Empty<int>();
    }
}
