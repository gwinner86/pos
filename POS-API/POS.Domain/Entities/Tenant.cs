using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Tenant : BaseEntity
    {
        public string TenantName { get; set; } = string.Empty;
        public int FeatureId { get; set; } // Default/Main Feature ID?
        public bool IsActive { get; set; } = true;
        
        // Navigation Properties
        public ICollection<Company> Companies { get; set; } = new List<Company>();
        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<TenantFeatureAssignment> FeatureAssignments { get; set; } = new List<TenantFeatureAssignment>();
    }
}
