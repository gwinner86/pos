using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS.Domain.Entities
{
    public class TenantFeatureAssignment
    {
        [Key]
        public int AssignmentId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? CompanyId { get; set; }
        public Guid? LocationId { get; set; }
        public int FeatureId { get; set; }
        public bool IsEnabled { get; set; } = false;

        public Tenant? Tenant { get; set; }
        public Feature? Feature { get; set; }
        // Optional relationships to Company/Location if needed loosely or strictly
        public Company? Company { get; set; }
        public Location? Location { get; set; }
    }
}
