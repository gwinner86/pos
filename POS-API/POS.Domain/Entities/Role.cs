using System.ComponentModel.DataAnnotations;

namespace POS.Domain.Entities
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public Guid? TenantId { get; set; }
        public Guid? CompanyId { get; set; }
        public string? Description { get; set; }
        
        public Tenant? Tenant { get; set; }
        public Company? Company { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
