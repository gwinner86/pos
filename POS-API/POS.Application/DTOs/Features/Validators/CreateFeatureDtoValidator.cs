using FluentValidation;
using POS.Application.DTOs.Features;

namespace POS.Application.DTOs.Features.Validators
{
    public class CreateFeatureDtoValidator : AbstractValidator<CreateFeatureDto>
    {
        public CreateFeatureDtoValidator()
        {
            RuleFor(x => x.FeatureName)
                .NotEmpty().WithMessage("Feature Name is required.")
                .MaximumLength(100).WithMessage("Feature Name must not exceed 100 characters.");
            
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
        }
    }
}
