using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Location : BaseEntity
    {
        public Guid TenantId { get; set; }
        public Guid CompanyId { get; set; }
        public int? FeatureId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationType { get; set; } = string.Empty; // e.g., 'Store', 'Warehouse'
        public string? AddressLine1 { get; set; }
        public Guid? CreatedBy { get; set; }
        public Guid? CurrencyId { get; set; }
        public int VatCalculationType { get; set; } = 1; // 1 = Exclusive, 2 = Inclusive

        public Tenant? Tenant { get; set; }
        public Company? Company { get; set; }
        public Currency? Currency { get; set; }
    }
}
