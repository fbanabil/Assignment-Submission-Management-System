namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using AssignmentSystem.Api.Models.Enums;
using FluentValidation;

public class UserUpdateDtoValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateDtoValidator()
    {
        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.FullName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid user role.")
            .When(x => x.Role.HasValue);
    }
}