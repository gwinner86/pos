using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class InvoicePayment : BaseEntity
    {
        public Guid SupplierInvoiceId { get; set; } // Or Customer Invoice
        public string SupplierOrCustomer { get; set; } = "SUPPLIER";
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "CASH";
        public Guid CreatedBy { get; set; }

        public SupplierInvoice? SupplierInvoice { get; set; }
    }
}
