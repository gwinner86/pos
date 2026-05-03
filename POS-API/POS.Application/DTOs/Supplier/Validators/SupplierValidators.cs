using FluentValidation;

namespace POS.Application.DTOs.Supplier.Validators
{
    public class CreateSupplierDtoValidator : AbstractValidator<CreateSupplierDto>
    {
        public CreateSupplierDtoValidator()
        {
            RuleFor(x => x.SupplierName)
                .NotEmpty().WithMessage("Supplier Name is required.")
                .MaximumLength(100);
            
            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Invalid Email Address.")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.Phone)
                .MaximumLength(20);
        }
    }

    public class UpdateSupplierDtoValidator : AbstractValidator<UpdateSupplierDto>
    {
        public UpdateSupplierDtoValidator()
        {
            RuleFor(x => x.SupplierName)
                .NotEmpty().WithMessage("Supplier Name is required.")
                .MaximumLength(100);

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Invalid Email Address.")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.Phone)
                .MaximumLength(20);
        }
    }
}
