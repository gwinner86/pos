using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class ProductAudit : BaseEntity
    {
        public Guid ProductId { get; set; }
        public string Action { get; set; } = string.Empty; // "Update", "Create" (if we want)
        public string Reason { get; set; } = string.Empty;
        public string? Changes { get; set; } // JSON of what changed?
        
        public Guid ChangedBy { get; set; } // UserId
        
        // Navigation (Optional, maybe weak link to avoid complex FK issues on historical data)
        // public Product? Product { get; set; } 
    }
}
