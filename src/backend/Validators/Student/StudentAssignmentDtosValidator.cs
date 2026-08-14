namespace Backend.Validators.Student;

using Backend.DTOs.StudentDTOs;
using FluentValidation;

public class StudentAssignmentFilterDtoValidator : AbstractValidator<StudentAssignmentFilterDto>
{
    public StudentAssignmentFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1).WithMessage("Page size must be at least 1.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");
    }
}

public class StudentSubmissionCreateDtoValidator : AbstractValidator<StudentSubmissionCreateDto>
{
    public StudentSubmissionCreateDtoValidator()
    {
        RuleFor(x => x.AssignmentId)
            .NotEqual(System.Guid.Empty).WithMessage("Assignment ID is required.");

        RuleFor(x => x)
            .Must(x => !string.IsNullOrWhiteSpace(x.SubmissionText) || !string.IsNullOrWhiteSpace(x.FileUrl))
            .WithMessage("Submission must contain either text content or a file URL.");
    }
}
