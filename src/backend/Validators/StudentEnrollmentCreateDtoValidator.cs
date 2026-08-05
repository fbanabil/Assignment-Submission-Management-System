namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using FluentValidation;

public class StudentEnrollmentCreateDtoValidator : AbstractValidator<StudentEnrollmentCreateDto>
{
    public StudentEnrollmentCreateDtoValidator()
    {
        RuleFor(x => x.StudentId)
            .NotEqual(Guid.Empty).WithMessage("Student ID must not be empty.");

        RuleFor(x => x.ClassId)
            .NotEqual(Guid.Empty).WithMessage("Class ID must not be empty.");
    }
}