using Ecommerce_Api.Models.Dto;
using FluentValidation;

namespace Ecommerce_Api.Validators
{
    internal sealed class AddCategoryRequestValidator : AbstractValidator<AddCategory>
    {
        public AddCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required.")
                .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.");
            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
            RuleFor(x => x.ParentCategoryId)
                .GreaterThanOrEqualTo(0).WithMessage("Parent Category ID must be a non-negative integer.");
        }
    }
}
