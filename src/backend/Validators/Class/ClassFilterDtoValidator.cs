namespace Backend.Validators.Class;

using Backend.DTOs.ClassDTOs;
using FluentValidation;

public class ClassFilterDtoValidator : AbstractValidator<ClassFilterDto>
{
    public ClassFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Section)
            .MaximumLength(50).WithMessage("Section filter must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Section));

        RuleFor(x => x.AcademicYear)
            .MaximumLength(50).WithMessage("Academic year filter must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.AcademicYear));
    }
}
