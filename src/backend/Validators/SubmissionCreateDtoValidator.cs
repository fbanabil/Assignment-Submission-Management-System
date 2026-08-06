namespace AssignmentSystem.Api.Validators;

using Backend.DTOs.SubmissionDTOs;
using FluentValidation;

public class SubmissionCreateDtoValidator : AbstractValidator<SubmissionCreateDto>
{
    public SubmissionCreateDtoValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEqual(Guid.Empty).WithMessage("Assignment ID must not be empty.");

        RuleFor(x => x.StudentId)
            .NotEqual(Guid.Empty).WithMessage("Student ID must not be empty.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrEmpty(x.SubmissionText) || !string.IsNullOrEmpty(x.FileUrl))
            .WithMessage("At least one of submission text or file URL must be provided.");

        RuleFor(x => x.SubmissionText)
            .MaximumLength(4000).WithMessage("Submission text must not exceed 4000 characters.")
            .When(x => !string.IsNullOrEmpty(x.SubmissionText));

        RuleFor(x => x.FileUrl)
            .MaximumLength(500).WithMessage("File URL must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.FileUrl));
    }
}