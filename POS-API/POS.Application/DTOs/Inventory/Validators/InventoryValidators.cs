using FluentValidation;

namespace POS.Application.DTOs.Inventory.Validators
{
    public class AdjustInventoryDtoValidator : AbstractValidator<AdjustInventoryDto>
    {
        public AdjustInventoryDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Reason is required for inventory adjustment.")
                .MinimumLength(5).WithMessage("Reason must be at least 5 characters long.");

            RuleFor(x => x.TransactionType)
                .NotEmpty()
                .Must(x => new[] { "StockIn", "StockOut", "Audit", "Return", "Correction" }.Contains(x))
                .WithMessage("Invalid Transaction Type. Allowed: StockIn, StockOut, Audit, Return, Correction");

            RuleFor(x => x.AdjustmentQuantity)
                .NotEqual(0).WithMessage("Adjustment quantity cannot be zero.");

            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant ID is required.");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location ID is required.");
        }
    }

    public class CreateInventoryDtoValidator : AbstractValidator<CreateInventoryDto>
    {
        public CreateInventoryDtoValidator()
        {
            RuleFor(x => x.ProductVariantId).NotEmpty().WithMessage("Product Variant ID is required.");
            RuleFor(x => x.LocationId).NotEmpty().WithMessage("Location ID is required.");
            RuleFor(x => x.InitialQuantity).GreaterThanOrEqualTo(0).WithMessage("Initial quantity cannot be negative.");
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative.");
        }
    }
}
