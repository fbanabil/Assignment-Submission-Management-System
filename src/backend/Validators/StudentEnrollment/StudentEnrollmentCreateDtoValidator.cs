namespace Backend.Validators.StudentEnrollment;

using Backend.DTOs.StudentEnrollmentDTOs;
using FluentValidation;

public class StudentEnrollmentCreateDtoValidator : AbstractValidator<StudentEnrollmentCreateDto>
{
    public StudentEnrollmentCreateDtoValidator()
    {
        RuleFor(x => x.StudentEmail)
            .NotEmpty().WithMessage("Student email is required.")
            .EmailAddress().WithMessage("A valid student email address is required.");

        RuleFor(x => x.ClassId)
            .NotEqual(Guid.Empty).WithMessage("Class ID must not be empty.");
    }
}