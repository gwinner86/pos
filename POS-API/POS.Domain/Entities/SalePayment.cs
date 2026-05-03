using POS.Domain.Common;

namespace POS.Domain.Entities
{
    public class SalePayment : BaseEntity
    {
        public Guid SaleId { get; set; }
        public int PaymentMethodId { get; set; }
        public decimal Amount { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }

        public Sale? SaleHeader { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
    }
}
