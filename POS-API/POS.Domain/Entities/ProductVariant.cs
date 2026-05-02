using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class ProductVariant : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string VariantName { get; set; } = string.Empty; // e.g. "Size L, Red"
        public string VariantSku { get; set; } = string.Empty; // Full SKU
        public string? Barcode { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
        public Guid? UpdatedBy { get; set; }

        public Product? Product { get; set; }
        
        // Navigation properties for Pricing/Inventory
        public ICollection<Inventory> InventoryItems { get; set; } = new List<Inventory>();
        public ICollection<Pricing> Pricings { get; set; } = new List<Pricing>();
        public ICollection<Cost> Costs { get; set; } = new List<Cost>();
    }
}
