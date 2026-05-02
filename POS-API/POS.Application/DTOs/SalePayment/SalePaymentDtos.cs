using FluentValidation;

namespace POS.Application.DTOs.SalePayment
{
    public class SalePaymentDto
    {
        public Guid Id { get; set; }
        public Guid SaleId { get; set; }
        public int PaymentMethodId { get; set; }
        public string PaymentMethodName { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; }
        public Guid CreatedBy { get; set; }
    }

    public class CreateSalePaymentDto
    {
        public Guid SaleId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    }
}
