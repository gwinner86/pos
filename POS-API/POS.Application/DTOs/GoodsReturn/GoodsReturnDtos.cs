using POS.Application.DTOs.Sale;

namespace POS.Application.DTOs.GoodsReturn
{
    public class GoodsReturnDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public Guid? SalesOrPurchaseId { get; set; }
        public string ReturnType { get; set; } = "SALE_RETURN";
        public DateTime ReturnDate { get; set; }
        public decimal TotalRefundAmount { get; set; }
        public string? ReturnReason { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GoodsReturnedDetailDto> Details { get; set; } = new();
    }

    public class GoodsReturnedDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public Guid? SaleItemId { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal RefundAmount { get; set; }
        public string ReturnOutcome { get; set; } = "Refund";
    }

    public class CreateGoodsReturnDto
    {
        public Guid LocationId { get; set; }
        public Guid? SalesOrPurchaseId { get; set; }
        public string ReturnType { get; set; } = "SALE_RETURN";
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;
        public string? ReturnReason { get; set; }
        public List<CreateGoodsReturnedDetailDto> Details { get; set; } = new();
    }

    public class CreateGoodsReturnedDetailDto
    {
        public Guid ProductVariantId { get; set; }
        public Guid? SaleItemId { get; set; }
        public decimal QuantityReturned { get; set; }
        public decimal RefundAmount { get; set; }
        public string ReturnOutcome { get; set; } = "Refund"; // Refund, Exchange, Credit
    }
}
