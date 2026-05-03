namespace POS.Application.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiration { get; set; }
        public Guid UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Guid TenantId { get; set; }
        public string Role { get; set; } = string.Empty;
        public Guid? CompanyId { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
