using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Auth
{
    public class RegisterUserRequest
    {
        [Required]
        public Guid TenantId { get; set; }

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;



        [Required]
        public Guid CompanyId { get; set; }

        [Required]
        public int RoleId { get; set; }
    }
}
