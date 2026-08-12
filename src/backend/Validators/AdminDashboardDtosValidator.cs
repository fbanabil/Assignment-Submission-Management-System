using Backend.DTOs;
using FluentValidation;

namespace Backend.Validators
{
    
    public class DashboardSummeryValidator : AbstractValidator<DashboardSummaryDto>
    {
        public DashboardSummeryValidator()
        {
            RuleFor(x => x.Users.TotalUsers)
                .GreaterThanOrEqualTo(0).WithMessage("Total users must be greater than or equal to 0.");
            RuleFor(x => x.Users.ActiveUsers)
                .GreaterThanOrEqualTo(0).WithMessage("Active users must be greater than or equal to 0.");
            RuleFor(x => x.Assignments.TotalAssignments)
                .GreaterThanOrEqualTo(0).WithMessage("Total assignments must be greater than or equal to 0.");
            RuleFor(x => x.Submissions.TotalSubmissions)
                .GreaterThanOrEqualTo(0).WithMessage("Total submissions must be greater than or equal to 0.");
        }
    }
}
