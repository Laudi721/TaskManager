namespace Forge.Desk.WebApi.Dtos
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int UsersCount { get; set; }
    }
}
