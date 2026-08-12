namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.Models.Enums;
using Backend.DTOs.UserDTOs;
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

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Phone number must be a valid E.164 format.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));

        RuleFor(x => x.IsActive)
            .NotNull().WithMessage("IsActive must be specified.")
            .When(x => x.IsActive.HasValue);

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id must not be empty.");

        RuleFor(x => x.Id)
            .Must(id => Guid.TryParse(id.ToString(), out _)).WithMessage("Id must be a valid GUID.");
    }
}