using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class Inventory : BaseEntity
    {
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid LocationId { get; set; }
        public decimal InitialQuantity { get; set; }
        public decimal Quantity { get; set; }
        public decimal ReorderLevel { get; set; }
        public decimal TotalQuantitySold { get; set; }
        public decimal TotalQuantityReturned { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Guid? UpdatedBy { get; set; }

        public ProductVariant? ProductVariant { get; set; }
        public Location? Location { get; set; }
        public Product? Product { get; set; }
    }
}
