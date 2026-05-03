using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Sale
{
    public class SaleDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string SaleNumber { get; set; } = string.Empty;
        public DateTime SaleDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<SaleDetailDto> Details { get; set; } = new();
    }

    public class SaleDetailDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal LineTotal { get; set; }
        public string? VatDetails { get; set; }
    }

    public class CreateSaleDto
    {
        public Guid LocationId { get; set; }
        public Guid? CustomerId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } = "Cash";
        public List<CreateSaleDetailDto> Details { get; set; } = new();
    }

    public class CreateSaleDetailDto
    {
        public Guid ProductVariantId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Allow override vs System Price? Plan says yes, validate >= 0
        public decimal Discount { get; set; }
    }
}
