namespace Backend.Validators.TeacherAssignment;

using Backend.DTOs.TeacherAssignmentDTOs;
using FluentValidation;
using System;

public class TeacherAssignmentCreateDtoValidator : AbstractValidator<TeacherAssignmentCreateDto>
{
    public TeacherAssignmentCreateDtoValidator()
    {
        RuleFor(x => x.TeacherId)
            .NotEqual(Guid.Empty).WithMessage("Teacher ID must not be empty.");

        RuleFor(x => x)
            .Must(x => x.ClassSubjectId != Guid.Empty ||
                      (x.ClassId.HasValue && x.ClassId.Value != Guid.Empty &&
                       x.SubjectId.HasValue && x.SubjectId.Value != Guid.Empty))
            .WithMessage("Either ClassSubjectId or both ClassId and SubjectId must be provided.");
    }
}