using FluentValidation;

namespace POS.Application.DTOs.Tenant.Validators
{
    public class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
    {
        public CreateTenantDtoValidator()
        {
            RuleFor(x => x.TenantName)
                .NotEmpty().WithMessage("Tenant Name is required.")
                .MaximumLength(100).WithMessage("Tenant Name must not exceed 100 characters.");

            RuleFor(x => x.FeatureId)
                .GreaterThan(0).WithMessage("Feature ID is required and must be greater than 0.");
        }
    }
}
