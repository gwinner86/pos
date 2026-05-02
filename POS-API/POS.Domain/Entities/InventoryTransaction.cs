using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class InventoryTransaction : BaseEntity
    {
        public Guid InventoryId { get; set; }
        public decimal QuantityChanged { get; set; } // + or -
        public decimal OldQuantity { get; set; }
        public decimal NewQuantity { get; set; }
        public string TransactionType { get; set; } = string.Empty; // "StockIn", "StockOut", "Audit"
        public string Reason { get; set; } = string.Empty;
        public Guid CreatedBy { get; set; }
        
        // Navigation
        // public Inventory? Inventory { get; set; }
    }
}
