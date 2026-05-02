using FluentValidation;

namespace POS.Application.DTOs.Pricing.Validators
{
    public class CreatePricingDtoValidator : AbstractValidator<CreatePricingDto>
    {
        public CreatePricingDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
        }
    }

    public class UpdatePricingDtoValidator : AbstractValidator<UpdatePricingDto>
    {
        public UpdatePricingDtoValidator()
        {
            RuleFor(x => x.Reason).NotEmpty().WithMessage("A reason is required to update the price.");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");
        }
    }
}
