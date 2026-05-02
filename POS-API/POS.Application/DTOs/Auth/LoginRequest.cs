using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Auth
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public Guid? TenantId { get; set; }
        public string? TenantName { get; set; }
    }
}
