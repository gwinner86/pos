using FluentValidation;

namespace POS.Application.DTOs.Company.Validators
{
    public class CreateCompanyDtoValidator : AbstractValidator<CreateCompanyDto>
    {
        public CreateCompanyDtoValidator()
        {
            RuleFor(x => x.TenantId)
                .NotEmpty().WithMessage("Tenant ID is required.");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Company Name is required.")
                .MaximumLength(150).WithMessage("Company Name must not exceed 150 characters.");

            RuleFor(x => x.FeatureId)
                .GreaterThan(0).WithMessage("Feature ID is required and must be greater than 0.");

            RuleFor(x => x.CompanyPrimaryPhoneNumber)
                .NotEmpty().WithMessage("Primary Phone Number is required.")
                .MaximumLength(20).WithMessage("Phone Number must not exceed 20 characters.");
            
            RuleFor(x => x.CompanyEmail)
                .EmailAddress().WithMessage("Invalid Email Address.")
                .When(x => !string.IsNullOrEmpty(x.CompanyEmail));
        }
    }
}
