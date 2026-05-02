using FluentValidation;
using POS.Application.DTOs.Role;

namespace POS.Application.DTOs.Role.Validators
{
    public class CreateRoleDtoValidator : AbstractValidator<CreateRoleDto>
    {
        public CreateRoleDtoValidator()
        {
            RuleFor(x => x.RoleName).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Description).MaximumLength(200);
        }
    }
}
