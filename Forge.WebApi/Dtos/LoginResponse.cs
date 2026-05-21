namespace Forge.WebApi.Dtos
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? UserId { get; set; }
        public string? Login { get; set; }
        public string? Name { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAtUtc { get; set; }
    }
}
