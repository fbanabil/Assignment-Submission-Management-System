using Backend.DTOs.TeacherDTOs;
using FluentValidation;

namespace Backend.Validators.Teacher;

public class TeacherDashboardFilterDtoValidator : AbstractValidator<TeacherDashboardFilterDto>
{
    public TeacherDashboardFilterDtoValidator()
    {
        RuleFor(x => x.TeacherEmail)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.TeacherEmail))
            .WithMessage("TeacherEmail must be a valid email address.");

        RuleFor(x => x.ClassName)
            .MaximumLength(100)
            .When(x => !string.IsNullOrWhiteSpace(x.ClassName))
            .WithMessage("ClassName cannot exceed 100 characters.");

        RuleFor(x => x.SubjectCode)
            .MaximumLength(50)
            .When(x => !string.IsNullOrWhiteSpace(x.SubjectCode))
            .WithMessage("SubjectCode cannot exceed 50 characters.");
    }
}
