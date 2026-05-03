using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Currency : BaseEntity
    {
        public string CurrencyCode { get; set; } = string.Empty; // e.g., USD, GHS
        public string CurrencySymbol { get; set; } = string.Empty; // e.g., $
        public string CurrencyName { get; set; } = string.Empty; // e.g., US Dollar

        public Guid CompanyId { get; set; }
        public Company? Company { get; set; }

        public Guid TenantId { get; set; }
        public bool IsActive { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
