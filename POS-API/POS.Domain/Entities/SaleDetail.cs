using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class SaleDetail : BaseEntity
    {
        public Guid SaleId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal LineTotal { get; set; }
        public string? VatDetails { get; set; }

        public Sale? Sale { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}
