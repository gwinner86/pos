using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class SupplierInvoice : BaseEntity
    {
        public Guid? GoodsReceiptId { get; set; }
        public Guid SupplierId { get; set; }
        public Guid CompanyId { get; set; }
        public Guid TenantId { get; set; } // Added for consistency
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPaid { get; set; }
        public Guid CreatedBy { get; set; }
        public string Status { get; set; } = "Unpaid";

        public GoodsReceipt? GoodsReceipt { get; set; }
        public Supplier? Supplier { get; set; }
        public Company? Company { get; set; }
    }
}
