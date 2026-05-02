using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public Guid TenantId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool RequiresPasswordChange { get; set; } = false;
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Tenant? Tenant { get; set; }
        public ICollection<UserCompanyAssignment> Assignments { get; set; } = new List<UserCompanyAssignment>();
    }
}
