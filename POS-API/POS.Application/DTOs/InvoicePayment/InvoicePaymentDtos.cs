using System.ComponentModel.DataAnnotations;

namespace POS.Application.DTOs.InvoicePayment
{
    public class InvoicePaymentDto
    {
        public Guid Id { get; set; }
        public Guid SupplierInvoiceId { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "CASH";
        public DateTime CreatedAt { get; set; }
    }

    public class CreateInvoicePaymentDto
    {
        public Guid SupplierInvoiceId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "CASH";
    }
}
