using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Pricing
{
    public class PricingDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty; // Sku or name
        public Guid LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime EffectiveDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePricingDto
    {
        public Guid ProductVariantId { get; set; }
        public Guid LocationId { get; set; }
        public decimal Price { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    public class UpdatePricingDto
    {
        public decimal Price { get; set; }
        public string Reason { get; set; } = string.Empty; // Mandatory for Audit
        public DateTime? EffectiveDate { get; set; }
    }
}
