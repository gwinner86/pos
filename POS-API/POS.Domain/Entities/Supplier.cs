using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Supplier : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string? ContactName { get; set; }
        public string? ContactEmail { get; set; }
        public string? Phone { get; set; }
        public string? Terms { get; set; } // 'Net 30'
        public bool IsActive { get; set; } = true;
        public Guid? CreatedBy { get; set; }

        public Company? Company { get; set; }
        
        public Guid? LocationId { get; set; }
        public Location? Location { get; set; }
    }
}
