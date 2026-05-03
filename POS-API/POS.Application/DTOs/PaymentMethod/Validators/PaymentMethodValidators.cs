using FluentValidation;

namespace POS.Application.DTOs.PaymentMethod.Validators
{
    public class CreatePaymentMethodDtoValidator : AbstractValidator<CreatePaymentMethodDto>
    {
        public CreatePaymentMethodDtoValidator()
        {
            RuleFor(x => x.MethodName).NotEmpty().WithMessage("Method Name is required.")
                .MaximumLength(50).WithMessage("Method Name cannot exceed 50 characters.");
            
            RuleFor(x => x.PaymentType).NotEmpty().WithMessage("Payment Type is required.")
                .Must(type => new[] { "CASH", "CARD", "TRANSFER", "OTHER" }.Contains(type.ToUpper()))
                .WithMessage("Payment Type must be CASH, CARD, TRANSFER, or OTHER.");
        }
    }

    public class UpdatePaymentMethodDtoValidator : AbstractValidator<UpdatePaymentMethodDto>
    {
        public UpdatePaymentMethodDtoValidator()
        {
            RuleFor(x => x.PaymentMethodId).GreaterThan(0).WithMessage("Valid Payment Method ID is required.");
            
            RuleFor(x => x.MethodName).NotEmpty().WithMessage("Method Name is required.")
                .MaximumLength(50).WithMessage("Method Name cannot exceed 50 characters.");

            RuleFor(x => x.PaymentType).NotEmpty().WithMessage("Payment Type is required.")
                 .Must(type => new[] { "CASH", "CARD", "TRANSFER", "OTHER" }.Contains(type.ToUpper()))
                 .WithMessage("Payment Type must be CASH, CARD, TRANSFER, or OTHER.");
        }
    }
}
