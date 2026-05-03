using FluentValidation;

namespace POS.Application.DTOs.GoodsReceipt.Validators
{
    public class CreateGoodsReceiptDtoValidator : AbstractValidator<CreateGoodsReceiptDto>
    {
        public CreateGoodsReceiptDtoValidator()
        {
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            RuleFor(x => x.Details).NotEmpty().WithMessage("Goods Receipt must contain at least one item.");
            RuleForEach(x => x.Details).SetValidator(new CreateGoodsReceiptDetailDtoValidator());
        }
    }

    public class CreateGoodsReceiptDetailDtoValidator : AbstractValidator<CreateGoodsReceiptDetailDto>
    {
        public CreateGoodsReceiptDetailDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.QuantityReceived).GreaterThan(0).WithMessage("Quantity Received must be greater than zero.");
            RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit Cost cannot be negative.");
        }
    }

    public class UpdateGoodsReceiptDtoValidator : AbstractValidator<UpdateGoodsReceiptDto>
    {
        public UpdateGoodsReceiptDtoValidator()
        {
            RuleForEach(x => x.Details).SetValidator(new UpdateGoodsReceiptDetailDtoValidator());
        }
    }

    public class UpdateGoodsReceiptDetailDtoValidator : AbstractValidator<UpdateGoodsReceiptDetailDto>
    {
        public UpdateGoodsReceiptDetailDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.QuantityReceived).GreaterThan(0).WithMessage("Quantity Received must be greater than zero.");
            RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit Cost cannot be negative.");
        }
    }
}
