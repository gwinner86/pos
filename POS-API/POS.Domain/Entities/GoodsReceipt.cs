using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class GoodsReceipt : BaseEntity
    {
        public Guid? PurchaseOrderId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid LocationId { get; set; }
        public Guid TenantId { get; set; }
        public Guid? SupplierId { get; set; }
        public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
        public decimal TotalReceivedAmount { get; set; }
        public string Status { get; set; } = "Pending";
        public bool IsInvoiced { get; set; } = false;
        public Guid CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
        public Guid? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        public PurchaseOrder? PurchaseOrder { get; set; }
        public Company? Company { get; set; }
        public Location? Location { get; set; }
        public Supplier? Supplier { get; set; }
        public ICollection<GoodsReceiptDetail> Details { get; set; } = new List<GoodsReceiptDetail>();
    }
}
