namespace POS.Application.DTOs.Auth
{
    public class UserResponse
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty; // Simplified for now
        public bool IsActive { get; set; }
        public bool RequiresPasswordChange { get; set; }
        public Guid TenantId { get; set; }
        public Guid? CompanyId { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedByName { get; set; }
        public List<string> Permissions { get; set; } = new();
    }
}
