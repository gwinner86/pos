using FluentValidation;

namespace POS.Application.DTOs.Product.Validators
{
    public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
    {
        public CreateProductDtoValidator()
        {
            RuleFor(x => x.ProductName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ProductSkuBase).NotEmpty().MaximumLength(50);
            RuleForEach(x => x.Variants).SetValidator(new CreateProductVariantDtoValidator());
        }
    }

    public class CreateProductVariantDtoValidator : AbstractValidator<CreateProductVariantDto>
    {
        public CreateProductVariantDtoValidator()
        {
            RuleFor(x => x.VariantName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.VariantSku).NotEmpty().MaximumLength(50);
        }
    }

    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.ProductName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.ProductSkuBase).NotEmpty().MaximumLength(50);
            RuleFor(x => x.UpdateReason)
                .NotEmpty().WithMessage("Update Reason is required.")
                .MinimumLength(5).WithMessage("Update Reason must be at least 5 characters long.");
        }
    }
}
