using FluentValidation;

namespace POS.Application.DTOs.PaymentMethod
{
    public class PaymentMethodDto
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "CASH";
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePaymentMethodDto
    {
        public string MethodName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "CASH"; // CASH, CARD, TRANSFER
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePaymentMethodDto
    {
        public int PaymentMethodId { get; set; }
        public string MethodName { get; set; } = string.Empty;
        public string PaymentType { get; set; } = "CASH";
        public bool IsActive { get; set; }
    }
}
