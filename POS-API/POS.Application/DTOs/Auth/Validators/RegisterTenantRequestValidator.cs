using FluentValidation;

namespace POS.Application.DTOs.Auth.Validators
{
    public class RegisterTenantRequestValidator : AbstractValidator<RegisterTenantRequest>
    {
        public RegisterTenantRequestValidator()
        {
            RuleFor(x => x.TenantName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.CompanyName).NotEmpty().MaximumLength(255);
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(255);
            
            RuleFor(x => x.FeatureId).GreaterThan(0).WithMessage("FeatureId is required.");
            RuleFor(x => x.CompanyPrimaryPhoneNumber).NotEmpty().MaximumLength(50);
            RuleFor(x => x.CompanyEmailAddress).EmailAddress().MaximumLength(255).When(x => !string.IsNullOrEmpty(x.CompanyEmailAddress));
            RuleFor(x => x.LocationName).NotEmpty().MaximumLength(255).WithMessage("Location/Branch Name is required.");
            
            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
                .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
                .Matches("[0-9]").WithMessage("Password must contain at least one number.")
                .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");
        }
    }
}
