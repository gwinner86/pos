using FluentValidation;

namespace POS.Application.DTOs.Auth.Validators
{
    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        public RegisterUserRequestValidator()
        {
            RuleFor(x => x.TenantId).NotEmpty();
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.Email).NotEmpty().EmailAddress();
            RuleFor(x => x.CompanyId).NotEmpty();
            RuleFor(x => x.RoleId).GreaterThan(0);
        }
    }
}
