using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Company : BaseEntity
    {
        public Guid TenantId { get; set; }
        public int? FeatureId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyAddress { get; set; }
        public string? CompanyPrimaryPhoneNumber { get; set; }
        public string? CompanyOtherPhoneNumbers { get; set; }
        public string? CompanyEmail { get; set; }
        public string? TaxId { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public Tenant? Tenant { get; set; }
        public ICollection<Location> Locations { get; set; } = new List<Location>();
        public ICollection<UserCompanyAssignment> UserAssignments { get; set; } = new List<UserCompanyAssignment>();
    }
}
