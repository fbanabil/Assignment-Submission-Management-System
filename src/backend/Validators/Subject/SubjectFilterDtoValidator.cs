namespace Backend.Validators.Subject;

using Backend.DTOs.SubjectDTOs;
using FluentValidation;

public class SubjectFilterDtoValidator : AbstractValidator<SubjectFilterDto>
{
    public SubjectFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Code filter must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.Code));
    }
}
