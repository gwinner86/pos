using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Pricing : BaseEntity
    {
        public Guid? ProductVariantId { get; set; }
        public Guid LocationId { get; set; }
        public Guid ProductId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public decimal Price { get; set; }
        public string PackType { get; set; } = "Single";
        public decimal QuantityReduction { get; set; }
        public decimal ReductionAmount { get; set; }
        public decimal ReductionPercentage { get; set; }
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public Guid CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }

        public ProductVariant? ProductVariant { get; set; }
        public Location? Location { get; set; }
    }
}
