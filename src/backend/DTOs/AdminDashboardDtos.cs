namespace Backend.DTOs
{
    public sealed class DashboardSummaryDto
    {
        public string DataSource { get; set; } = string.Empty;
        public DateTime FetchedAt { get; set; }
        public UserSummaryDto Users { get; set; } = new();
        public AssignmentSummaryDto Assignments { get; set; } = new();
        public SubmissionSummaryDto Submissions { get; set; } = new();
    }

    public sealed class UserSummaryDto
    {
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int InactiveUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public List<UserRoleSummaryDto> RoleBreakdown { get; set; } = new();
    }

    public sealed class UserRoleSummaryDto
    {
        public string Role { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class AssignmentSummaryDto
    {
        public int TotalAssignments { get; set; }
        public int ActiveAssignments { get; set; }
        public int DraftAssignments { get; set; }
        public int DueSoonAssignments { get; set; }
        public int CompletionRate { get; set; }
        public List<AssignmentStatusSummaryDto> StatusBreakdown { get; set; } = new();
    }

    public sealed class AssignmentStatusSummaryDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class SubmissionSummaryDto
    {
        public int TotalSubmissions { get; set; }
        public int SubmittedToday { get; set; }
        public int PendingReview { get; set; }
        public int GradedSubmissions { get; set; }
        public List<SubmissionVolumeDto> WeeklyVolumes { get; set; } = new();
    }

    public sealed class SubmissionVolumeDto
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
