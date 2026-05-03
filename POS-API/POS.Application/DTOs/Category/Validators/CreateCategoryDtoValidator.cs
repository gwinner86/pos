using FluentValidation;

namespace POS.Application.DTOs.Category.Validators
{
    public class CreateCategoryDtoValidator : AbstractValidator<CreateCategoryDto>
    {
        public CreateCategoryDtoValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("Category Name is required.")
                .MaximumLength(100).WithMessage("Category Name must not exceed 100 characters.");

            // Optional: Validations for IDs are usually handled by Service/DB constraints 
            // or a comprehensive checking mechanism. Simple NotEmpty is not enough for Guids.
        }
    }
}
