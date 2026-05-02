using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Cost : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid? LocationId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public Guid SupplierId { get; set; }
        public decimal CostValue { get; set; } // Renamed from 'Cost' to avoid class name conflict
        public DateTime EffectiveDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }

        public ProductVariant? ProductVariant { get; set; }
        public Location? Location { get; set; }
        public Supplier? Supplier { get; set; }
    }
}
