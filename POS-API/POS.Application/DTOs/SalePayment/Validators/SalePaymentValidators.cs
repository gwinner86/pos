using FluentValidation;

namespace POS.Application.DTOs.SalePayment.Validators
{
    public class CreateSalePaymentDtoValidator : AbstractValidator<CreateSalePaymentDto>
    {
        public CreateSalePaymentDtoValidator()
        {
            RuleFor(x => x.SaleId).NotEmpty().WithMessage("Sale ID is required.");
            RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Valid Payment Method is required.");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Payment amount must be greater than zero.");
        }
    }
}
