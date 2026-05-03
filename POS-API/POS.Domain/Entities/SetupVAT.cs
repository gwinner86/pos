using System.ComponentModel.DataAnnotations;
using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class SetupVAT : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid? LocationId { get; set; } // Nullable if VAT applies company-wide

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal Rate { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(10)]
        public string? CountryCode { get; set; }

        public Guid? MakerId { get; set; }
        public DateTime? MakeDate { get; set; }
        
        public bool IsActive { get; set; } = true;

        // Navigation properties
        public Tenant? Tenant { get; set; }
        public Company? Company { get; set; }
        public Location? Location { get; set; }
    }
}
