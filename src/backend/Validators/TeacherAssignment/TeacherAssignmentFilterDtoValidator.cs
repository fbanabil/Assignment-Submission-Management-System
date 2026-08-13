namespace Backend.Validators.TeacherAssignment;

using Backend.DTOs.TeacherAssignmentDTOs;
using FluentValidation;

public class TeacherAssignmentFilterDtoValidator : AbstractValidator<TeacherAssignmentFilterDto>
{
    public TeacherAssignmentFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.TeacherName)
            .MaximumLength(100).WithMessage("Teacher name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.TeacherName));

        RuleFor(x => x.TeacherEmail)
            .MaximumLength(100).WithMessage("Teacher email filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.TeacherEmail));

        RuleFor(x => x.ClassName)
            .MaximumLength(100).WithMessage("Class name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ClassName));

        RuleFor(x => x.SubjectCode)
            .MaximumLength(50).WithMessage("Subject code filter must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.SubjectCode));
    }
}
