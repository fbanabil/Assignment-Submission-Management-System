namespace Backend.Validators.Class;

using Backend.DTOs.ClassSubjectDTOs;
using FluentValidation;

public class ClassSubjectCreateDtoValidator : AbstractValidator<ClassSubjectCreateDto>
{
    public ClassSubjectCreateDtoValidator()
    {
        RuleFor(x => x.ClassId)
            .NotEqual(Guid.Empty).WithMessage("Class ID must not be empty.");

        RuleFor(x => x.SubjectId)
            .NotEqual(Guid.Empty).WithMessage("Subject ID must not be empty.");
    }
}