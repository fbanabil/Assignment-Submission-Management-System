namespace Backend.Validators.User;

using Backend.DTOs.UserDTOs;
using FluentValidation;

public class UserFilterDtoValidator : AbstractValidator<UserFilterDto>
{
    public UserFilterDtoValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid user role.")
            .When(x => x.Role.HasValue);

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Email)
            .MaximumLength(100).WithMessage("Email filter must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Email));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(50).WithMessage("Phone number filter must not exceed 50 characters.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}
