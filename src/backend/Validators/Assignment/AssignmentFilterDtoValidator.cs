namespace Backend.Validators.Assignment;

using Backend.DTOs.AssignmentDTOs;
using FluentValidation;

public class AssignmentFilterDtoValidator : AbstractValidator<AssignmentFilterDto>
{
    public AssignmentFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Title)
            .MaximumLength(150).WithMessage("Title filter must not exceed 150 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.ClassName)
            .MaximumLength(100).WithMessage("Class name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.ClassName));

        RuleFor(x => x.TeacherName)
            .MaximumLength(100).WithMessage("Teacher name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.TeacherName));

        RuleFor(x => x.TeacherEmail)
            .MaximumLength(100).WithMessage("Teacher email filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.TeacherEmail));
    }
}
