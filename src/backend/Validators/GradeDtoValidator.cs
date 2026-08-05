namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using FluentValidation;

public class GradeDtoValidator : AbstractValidator<GradeDto>
{
    public GradeDtoValidator()
    {
        RuleFor(x => x.SubmissionId)
            .NotEqual(Guid.Empty).WithMessage("Submission ID must not be empty.");

        RuleFor(x => x.Marks)
            .GreaterThanOrEqualTo(0).WithMessage("Marks must be greater than or equal to 0.");

        RuleFor(x => x.Feedback)
            .MaximumLength(1000).WithMessage("Feedback must not exceed 1000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Feedback));
    }
}