using FluentValidation;

namespace POS.Application.DTOs.Sale.Validators
{
    public class CreateSaleDtoValidator : AbstractValidator<CreateSaleDto>
    {
        public CreateSaleDtoValidator()
        {
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            RuleFor(x => x.Details).NotEmpty().WithMessage("Sale must contain at least one item.");
            RuleForEach(x => x.Details).SetValidator(new CreateSaleDetailDtoValidator());
        }
    }

    public class CreateSaleDetailDtoValidator : AbstractValidator<CreateSaleDetailDto>
    {
        public CreateSaleDetailDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).WithMessage("Unit Price cannot be negative.");
            RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).WithMessage("Discount cannot be negative.");
        }
    }
}
