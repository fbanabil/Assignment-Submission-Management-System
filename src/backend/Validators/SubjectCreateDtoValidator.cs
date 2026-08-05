namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using FluentValidation;
using System.Text.RegularExpressions;

public class SubjectCreateDtoValidator : AbstractValidator<SubjectCreateDto>
{
    public SubjectCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Subject name is required.")
            .MaximumLength(100).WithMessage("Subject name must not exceed 100 characters.");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Subject code is required.")
            .MaximumLength(50).WithMessage("Subject code must not exceed 50 characters.")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Subject code must contain only alphanumeric characters.");
    }
}