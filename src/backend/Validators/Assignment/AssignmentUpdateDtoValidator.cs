namespace Backend.Validators.Assignment;

using Backend.DTOs.AssignmentDTOs;
using FluentValidation;

public class AssignmentUpdateDtoValidator : AbstractValidator<AssignmentUpdateDto>
{
    public AssignmentUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Assignment title must not exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.Title));

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Assignment description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.MaxMarks)
            .GreaterThan(0).WithMessage("Maximum marks must be greater than 0.")
            .When(x => x.MaxMarks.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Status must be a valid assignment status.")
            .When(x => x.Status.HasValue);
    }
}