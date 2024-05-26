using FluentValidation;
using SMS.Core.Model;

namespace SMS.Core.Validator;

public class CategoryModelValidator : AbstractValidator<CategoryModel>
{
	public CategoryModelValidator()
	{
		RuleFor(p => p.Name)
			.NotEmpty().WithMessage("Please enter 'Name'.")
			.MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
			.MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");
	}
}