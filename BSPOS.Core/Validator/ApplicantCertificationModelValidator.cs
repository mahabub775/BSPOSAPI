﻿using FluentValidation;
using SMS.Core.Model;

namespace SMS.Core.Validator;

public class ApplicantCertificationModelValidator : AbstractValidator<ApplicantCertificationModel>
{
	public ApplicantCertificationModelValidator()
	{
		RuleFor(p => p.CertificateNumber)
			.NotEmpty().WithMessage("Please enter 'Certificate Number'.")
			.MinimumLength(3).WithMessage("Minimum length of 'Certificate Number' is 3 characters.")
			.MaximumLength(100).WithMessage("Maximum length of 'Certificate Number' is 100 characters.");

	}
}