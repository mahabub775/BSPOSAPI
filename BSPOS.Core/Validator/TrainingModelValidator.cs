using FluentValidation;
using SMS.Core.Model;

namespace SMS.Core.Validator;

public class TrainingModelValidator : AbstractValidator<TrainingModel>
{
	public TrainingModelValidator()
	{
		RuleFor(p => p.TrainingName)
			.NotEmpty().WithMessage("Please enter 'Name'.")
			.MinimumLength(3).WithMessage("Minimum length of 'Name' is 3 characters.")
			.MaximumLength(150).WithMessage("Maximum length of 'Name' is 150 characters.");

	}
}