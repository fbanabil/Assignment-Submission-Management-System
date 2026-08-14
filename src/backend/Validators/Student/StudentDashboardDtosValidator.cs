using Backend.DTOs.StudentDTOs;
using FluentValidation;

namespace Backend.Validators.Student
{
    public class StudentDashboardResponseDtoValidator : AbstractValidator<StudentDashboardResponseDto>
    {
        public StudentDashboardResponseDtoValidator()
        {
            RuleFor(x => x.EnrolledClassesCount)
                .GreaterThanOrEqualTo(0).WithMessage("Enrolled classes count must be greater than or equal to 0.");

            RuleFor(x => x.PendingAssignmentsCount)
                .GreaterThanOrEqualTo(0).WithMessage("Pending assignments count must be greater than or equal to 0.");

            RuleFor(x => x.CompletedAssignmentsCount)
                .GreaterThanOrEqualTo(0).WithMessage("Completed assignments count must be greater than or equal to 0.");

            RuleFor(x => x.AverageGrade)
                .GreaterThanOrEqualTo(0.0).WithMessage("Average grade must be greater than or equal to 0.");
        }
    }

    public class StudentAssignmentDueDtoValidator : AbstractValidator<StudentAssignmentDueDto>
    {
        public StudentAssignmentDueDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Assignment title is required.");

            RuleFor(x => x.SubjectName)
                .NotEmpty().WithMessage("Subject name is required.");

            RuleFor(x => x.MaxMarks)
                .GreaterThan(0).WithMessage("Max marks must be greater than 0.");
        }
    }

    public class StudentRecentGradeDtoValidator : AbstractValidator<StudentRecentGradeDto>
    {
        public StudentRecentGradeDtoValidator()
        {
            RuleFor(x => x.AssignmentTitle)
                .NotEmpty().WithMessage("Assignment title is required.");

            RuleFor(x => x.MaxMarks)
                .GreaterThan(0).WithMessage("Max marks must be greater than 0.");
        }
    }
}
