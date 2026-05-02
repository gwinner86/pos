using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.GoodsReceipt
{
    public class GoodsReceiptDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public Guid? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid? PurchaseOrderId { get; set; }
        public DateTime ReceiptDate { get; set; }
        public decimal TotalReceivedAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsInvoiced { get; set; }
        public string IsInvoicedYesNo => IsInvoiced ? "YES" : "NO";
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<GoodsReceiptDetailDto> Details { get; set; } = new();
    }

    public class GoodsReceiptDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public decimal QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreateGoodsReceiptDto
    {
        public Guid LocationId { get; set; }
        public Guid? SupplierId { get; set; }
        public Guid? PurchaseOrderId { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public List<CreateGoodsReceiptDetailDto> Details { get; set; } = new();
    }

    public class CreateGoodsReceiptDetailDto
    {
        public Guid ProductVariantId { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class UpdateGoodsReceiptDto
    {
        public DateTime? ReceiptDate { get; set; }
        public List<UpdateGoodsReceiptDetailDto> Details { get; set; } = new();
    }

    public class UpdateGoodsReceiptDetailDto
    {
        public Guid? Id { get; set; }
        public Guid ProductVariantId { get; set; }
        public decimal QuantityReceived { get; set; }
        public decimal UnitCost { get; set; }
    }
}
