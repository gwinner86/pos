using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.PurchaseOrder
{
    public class PurchaseOrderDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string PoNumber { get; set; } = string.Empty;
        public Guid? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<PurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class PurchaseOrderDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty; // Sku or name
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
        public decimal QuantityReceived { get; set; }
    }

    public class CreatePurchaseOrderDto
    {
        public Guid LocationId { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class CreatePurchaseOrderDetailDto
    {
        public Guid ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }

    public class UpdatePurchaseOrderDto
    {
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public List<UpdatePurchaseOrderDetailDto> Details { get; set; } = new();
    }

    public class UpdatePurchaseOrderDetailDto
    {
        public Guid? Id { get; set; } // If null, it's a new item added during update
        public Guid ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitCost { get; set; }
    }
}
