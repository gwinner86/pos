using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class GoodsReturnedDetail : BaseEntity
    {
        public Guid ReturnHeaderId { get; set; }
        public Guid ProductVariantId { get; set; }
        public Guid? ProductId { get; set; }
        public Guid? SaleItemId { get; set; } // Link to original sale item
        public decimal QuantityReturned { get; set; }
        public decimal RefundAmount { get; set; }
        public string ReturnOutcome { get; set; } = "Refund";
        public Guid CreatedBy { get; set; }

        public GoodsReturn? ReturnHeader { get; set; }
        public ProductVariant? ProductVariant { get; set; }
        public SaleDetail? SaleItem { get; set; }
    }
}
