using System.ComponentModel.DataAnnotations;
using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class SetupCountry : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CountryName { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string CountryCode { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Tenant? Tenant { get; set; }
        public Company? Company { get; set; }
        public Location? Location { get; set; }
    }
}
