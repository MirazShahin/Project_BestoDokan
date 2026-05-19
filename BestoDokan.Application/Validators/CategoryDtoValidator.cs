using FluentValidation;
using BestoDokan.Application.DTOs;

namespace BestoDokan.Application.Validators
{
    public class CategoryDtoValidator : AbstractValidator<CategoryDto>
    {
        public CategoryDtoValidator()
        {
            RuleFor(c => c.Name)
                .NotEmpty().WithMessage("Category name cannot be empty.")
                .MaximumLength(50).WithMessage("Category name cannot exceed 50 characters.");
        }
    }
}