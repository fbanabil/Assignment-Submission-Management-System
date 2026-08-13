namespace Backend.Validators.Submission;

using Backend.DTOs.SubmissionDTOs;
using FluentValidation;

public class SubmissionFilterDtoValidator : AbstractValidator<SubmissionFilterDto>
{
    public SubmissionFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.ClassName)
            .MaximumLength(100).WithMessage("Class name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ClassName));

        RuleFor(x => x.SubjectName)
            .MaximumLength(100).WithMessage("Subject name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.SubjectName));

        RuleFor(x => x.SubjectCode)
            .MaximumLength(50).WithMessage("Subject code filter must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.SubjectCode));

        RuleFor(x => x.AssignmentTitle)
            .MaximumLength(150).WithMessage("Assignment title filter must not exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.AssignmentTitle));

        RuleFor(x => x.StudentName)
            .MaximumLength(100).WithMessage("Student name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.StudentName));

        RuleFor(x => x.StudentEmail)
            .MaximumLength(100).WithMessage("Student email filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.StudentEmail));
    }
}
