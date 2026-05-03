using FluentValidation;

namespace POS.Application.DTOs.InvoicePayment.Validators
{
    public class CreateInvoicePaymentDtoValidator : AbstractValidator<CreateInvoicePaymentDto>
    {
        public CreateInvoicePaymentDtoValidator()
        {
            RuleFor(x => x.SupplierInvoiceId).NotEmpty().WithMessage("Supplier Invoice ID is required.");
            RuleFor(x => x.AmountPaid).GreaterThan(0).WithMessage("Amount Paid must be greater than zero.");
            RuleFor(x => x.PaymentMethod).NotEmpty().WithMessage("Payment Method is required.");
            RuleFor(x => x.PaymentDate).NotEmpty().WithMessage("Payment Date is required.");
        }
    }
}
