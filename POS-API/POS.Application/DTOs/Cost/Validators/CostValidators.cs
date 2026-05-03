using FluentValidation;

namespace POS.Application.DTOs.Cost.Validators
{
    public class CreateCostDtoValidator : AbstractValidator<CreateCostDto>
    {
        public CreateCostDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.SupplierId).NotEmpty().WithMessage("Supplier is required.");
            RuleFor(x => x.CostValue).GreaterThanOrEqualTo(0).WithMessage("Cost Value cannot be negative.");
        }
    }

    public class UpdateCostDtoValidator : AbstractValidator<UpdateCostDto>
    {
        public UpdateCostDtoValidator()
        {
            RuleFor(x => x.CostValue).GreaterThanOrEqualTo(0)
                .When(x => x.CostValue.HasValue)
                .WithMessage("Cost Value cannot be negative.");
        }
    }
}
