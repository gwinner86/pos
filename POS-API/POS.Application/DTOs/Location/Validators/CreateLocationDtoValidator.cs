using FluentValidation;

namespace POS.Application.DTOs.Location.Validators
{
    public class CreateLocationDtoValidator : AbstractValidator<CreateLocationDto>
    {
        public CreateLocationDtoValidator()
        {
            RuleFor(x => x.LocationName).NotEmpty().MaximumLength(100);

            RuleFor(x => x.LocationType).NotEmpty().MaximumLength(50);
        }
    }
}
