namespace AssignmentSystem.Api.Validators;

using AssignmentSystem.Api.DTOs;
using FluentValidation;

public class TeacherAssignmentCreateDtoValidator : AbstractValidator<TeacherAssignmentCreateDto>
{
    public TeacherAssignmentCreateDtoValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEqual(Guid.Empty).WithMessage("Teacher ID must not be empty.");

        RuleFor(x => x.ClassSubjectId)
            .NotEqual(Guid.Empty).WithMessage("Class Subject ID must not be empty.");
    }
}