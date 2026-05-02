using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; }
        public Guid LocationId { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpectedDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? LastUpdated { get; set; }
        public Guid? UpdatedBy { get; set; }
        public string Status { get; set; } = "Pending";
        public string? Notes { get; set; }

        public Company? Company { get; set; }
        public Location? Location { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<PurchaseOrderDetail> Details { get; set; } = new List<PurchaseOrderDetail>();
    }
}
