using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Product : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSkuBase { get; set; } = string.Empty;    
        public Guid? CategoryId { get; set; }   
        public Guid? LocationId { get; set; }
        public Guid? SupplierId { get; set; } // Global/Default Supplier
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVatExcluded { get; set; } = false;
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }

        public Company? Company { get; set; }
        public Category? Category { get; set; }
        public Location? Location { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}
