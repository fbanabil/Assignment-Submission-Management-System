namespace Backend.Validators.Assignment;

using Backend.DTOs.AssignmentDTOs;
using FluentValidation;

public class AssignmentCreateDtoValidator : AbstractValidator<AssignmentCreateDto>
{
    public AssignmentCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Assignment title is required.")
            .MaximumLength(200).WithMessage("Assignment title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Assignment description is required.")
            .MaximumLength(2000).WithMessage("Assignment description must not exceed 2000 characters.");

        RuleFor(x => x.ClassId)
            .NotEmpty().WithMessage("Class selection is required.");

        RuleFor(x => x.SubjectId)
            .NotEmpty().WithMessage("Subject selection is required.");

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0).WithMessage("Maximum marks must be greater than 0.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid assignment status.");
    }
}
