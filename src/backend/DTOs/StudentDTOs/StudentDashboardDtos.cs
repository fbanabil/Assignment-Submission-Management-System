namespace Backend.DTOs.StudentDTOs
{
    public class StudentAssignmentDueDto
    {
        public Guid AssignmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public DateTime DueDate { get; set; }
        public int MaxMarks { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class StudentRecentGradeDto
    {
        public Guid SubmissionId { get; set; }
        public string AssignmentTitle { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public string SubjectCode { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? GradedAt { get; set; }
        public int? Grade { get; set; }
        public int MaxMarks { get; set; }
        public string Feedback { get; set; } = string.Empty;
        public string GradedByTeacherName { get; set; } = string.Empty;
    }

    public class StudentDashboardResponseDto
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public int EnrolledClassesCount { get; set; }
        public int PendingAssignmentsCount { get; set; }
        public int CompletedAssignmentsCount { get; set; }
        public double AverageGrade { get; set; }
        public List<StudentAssignmentDueDto> AssignmentsDueSoon { get; set; } = new();
        public List<StudentRecentGradeDto> RecentGradesFeedback { get; set; } = new();
        public string DataSource { get; set; } = "Server API";
        public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
    }

    public class StudentDashboardFilterDto
    {
        public Guid? StudentId { get; set; }
    }
}
