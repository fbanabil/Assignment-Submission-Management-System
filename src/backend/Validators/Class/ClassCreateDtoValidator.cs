namespace Backend.Validators.Class;

using Backend.DTOs.ClassDTOs;
using FluentValidation;

public class ClassCreateDtoValidator : AbstractValidator<ClassCreateDto>
{
    public ClassCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Class name is required.")
            .MaximumLength(100).WithMessage("Class name must not exceed 100 characters.");

        RuleFor(x => x.Section)
            .NotEmpty().WithMessage("Section is required.")
            .MaximumLength(50).WithMessage("Section must not exceed 50 characters.");

        RuleFor(x => x.AcademicYear)
            .NotEmpty().WithMessage("Academic year is required.")
            .MaximumLength(50).WithMessage("Academic year must not exceed 50 characters.");
    }
}