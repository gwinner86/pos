using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.Inventory
{
    public class InventoryDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string? ProductName { get; set; }
        public Guid ProductVariantId { get; set; }
        public string? VariantName { get; set; }
        public Guid LocationId { get; set; }
        public string? LocationName { get; set; }
        public decimal Quantity { get; set; }
        public DateTime? LastUpdated { get; set; }
    }

    public class AdjustInventoryDto
    {
        public Guid ProductVariantId { get; set; } // Required to identify what
        public Guid LocationId { get; set; }       // Required to identify where
        
        public decimal AdjustmentQuantity { get; set; } // Can be negative for stock out
        public string TransactionType { get; set; } = string.Empty; // "StockIn", "StockOut", "Audit", "Return"
        
        // REQUIRED
        public string Reason { get; set; } = string.Empty;
    }

    public class CreateInventoryDto
    {
        public Guid ProductVariantId { get; set; }
        public Guid LocationId { get; set; }
        public decimal InitialQuantity { get; set; }
        public decimal Quantity { get; set; }
    }
}
