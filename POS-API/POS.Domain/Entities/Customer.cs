using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; } // Added for consistency
        public string CustomerCode { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? CompanyName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public bool IsActive { get; set; } = true;
        public decimal LoyaltyPoints { get; set; }
        public Guid? CreatedBy { get; set; }

        public Company? Company { get; set; }
    }
}
