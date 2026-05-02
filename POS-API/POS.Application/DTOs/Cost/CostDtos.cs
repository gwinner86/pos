using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Cost
{
    public class CostDto
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public Guid ProductVariantId { get; set; }
        public string VariantName { get; set; } = string.Empty; // Sku or name
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public Guid? LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public decimal CostValue { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCostDto
    {
        public Guid ProductVariantId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid? LocationId { get; set; }
        public decimal CostValue { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }

    public class UpdateCostDto
    {
        public decimal? CostValue { get; set; }
        public DateTime? EffectiveDate { get; set; }
    }
}
