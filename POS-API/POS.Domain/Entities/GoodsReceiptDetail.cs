using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class GoodsReceiptDetail : BaseEntity
    {
        public Guid GoodsReceiptId { get; set; }
        public Guid ProductId { get; set; }
        public Guid ProductVariantId { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }

        public GoodsReceipt? GoodsReceipt { get; set; }
        public Product? Product { get; set; }
        public ProductVariant? ProductVariant { get; set; }
    }
}
