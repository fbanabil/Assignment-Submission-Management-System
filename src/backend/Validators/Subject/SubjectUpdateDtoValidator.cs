namespace Backend.Validators.Subject;

using Backend.DTOs.SubjectDTOs;
using FluentValidation;

public class SubjectUpdateDtoValidator : AbstractValidator<SubjectUpdateDto>
{
    public SubjectUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Subject name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Code)
            .MaximumLength(50).WithMessage("Subject code must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Subject code must contain only alphanumeric characters.")
            .When(x => !string.IsNullOrEmpty(x.Code));
    }
}