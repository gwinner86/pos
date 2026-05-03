using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Role
{
    public class CreateRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? CompanyId { get; set; }
    }

    public class UpdateRoleDto
    {
        [Required]
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class RoleResponse
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? TenantId { get; set; }
        public Guid? CompanyId { get; set; }
    }
}
