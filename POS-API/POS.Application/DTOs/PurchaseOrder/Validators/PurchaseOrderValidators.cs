using FluentValidation;

namespace POS.Application.DTOs.PurchaseOrder.Validators
{
    public class CreatePurchaseOrderDtoValidator : AbstractValidator<CreatePurchaseOrderDto>
    {
        public CreatePurchaseOrderDtoValidator()
        {
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location is required.");
            RuleFor(x => x.Details).NotEmpty().WithMessage("Purchase Order dates must contain at least one item.");
            RuleForEach(x => x.Details).SetValidator(new CreatePurchaseOrderDetailDtoValidator());
        }
    }

    public class CreatePurchaseOrderDetailDtoValidator : AbstractValidator<CreatePurchaseOrderDetailDto>
    {
        public CreatePurchaseOrderDetailDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit Cost cannot be negative.");
        }
    }

    public class UpdatePurchaseOrderDtoValidator : AbstractValidator<UpdatePurchaseOrderDto>
    {
        public UpdatePurchaseOrderDtoValidator()
        {
            RuleForEach(x => x.Details).SetValidator(new UpdatePurchaseOrderDetailDtoValidator());
        }
    }

    public class UpdatePurchaseOrderDetailDtoValidator : AbstractValidator<UpdatePurchaseOrderDetailDto>
    {
        public UpdatePurchaseOrderDetailDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant is required.");
            RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantity must be greater than zero.");
            RuleFor(x => x.UnitCost).GreaterThanOrEqualTo(0).WithMessage("Unit Cost cannot be negative.");
        }
    }
}
