namespace Backend.Validators.User;

using AssignmentSystem.Api.Models.Enums;
using Backend.DTOs.UserDTOs;
using FluentValidation;

public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
{
    public UserCreateDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(255).WithMessage("Full name must not exceed 255 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid user role.");

        RuleFor(x=>x.PhoneNumber)
            .NotNull().WithMessage("Phone number is required.")
            // Only + and digits allowed
            .Must(phone => System.Text.RegularExpressions.Regex.IsMatch(phone, @"^\+?\d+$")).WithMessage("Phone number must contain only digits and may start with a +.");

        RuleFor(x=>x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Confirm password must match the password.");
    }
}