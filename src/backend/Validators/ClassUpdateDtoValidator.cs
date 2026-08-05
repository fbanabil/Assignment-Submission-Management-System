namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using FluentValidation;

public class ClassUpdateDtoValidator : AbstractValidator<ClassUpdateDto>
{
    public ClassUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Class name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Section)
            .MaximumLength(50).WithMessage("Section must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Section));

        RuleFor(x => x.AcademicYear)
            .MaximumLength(50).WithMessage("Academic year must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.AcademicYear));
    }
}