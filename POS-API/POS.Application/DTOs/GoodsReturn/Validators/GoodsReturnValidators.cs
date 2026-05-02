using FluentValidation;

namespace POS.Application.DTOs.GoodsReturn.Validators
{
    public class CreateGoodsReturnDtoValidator : AbstractValidator<CreateGoodsReturnDto>
    {
        public CreateGoodsReturnDtoValidator()
        {
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location ID is required.");
            RuleFor(x => x.Details).NotEmpty().WithMessage("Return actions must include at least one item.");
            RuleForEach(x => x.Details).SetValidator(new CreateGoodsReturnedDetailDtoValidator());
        }
    }

    public class CreateGoodsReturnedDetailDtoValidator : AbstractValidator<CreateGoodsReturnedDetailDto>
    {
        public CreateGoodsReturnedDetailDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant ID is required.");
            RuleFor(x => x.QuantityReturned).GreaterThan(0).WithMessage("Quantity returned must be greater than zero.");
            RuleFor(x => x.RefundAmount).GreaterThanOrEqualTo(0).WithMessage("Refund amount cannot be negative.");
        }
    }
}
