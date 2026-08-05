namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using FluentValidation;

public class SubmissionUpdateDtoValidator : AbstractValidator<SubmissionUpdateDto>
{
    public SubmissionUpdateDtoValidator()
    {
        RuleFor(x => x.SubmissionText)
            .MaximumLength(4000).WithMessage("Submission text must not exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.SubmissionText));

        RuleFor(x => x.FileUrl)
            .MaximumLength(500).WithMessage("File URL must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.FileUrl));
    }
}