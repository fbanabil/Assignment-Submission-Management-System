namespace Backend.DTOs.TeacherDTOs;

public class TeacherAssignedClassSubjectDto
{
    public Guid ClassId { get; set; }
    public Guid SubjectId { get; set; }
    public string ClassSubjectId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string ClassSection { get; set; } = string.Empty;
    public string AcademicYear { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public int StudentCount { get; set; }
}

public class TeacherUpcomingDeadlineDto
{
    public string AssignmentId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string SubjectCode { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public int TotalSubmissions { get; set; }
    public int UngradedSubmissions { get; set; }
}

public class TeacherDashboardFilterDto
{
    public string? TeacherEmail { get; set; }
    public string? ClassName { get; set; }
    public string? SubjectCode { get; set; }
}

public class TeacherDashboardResponseDto
{
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public int TotalAssignedClasses { get; set; }
    public int ActiveAssignmentsCount { get; set; }
    public int UngradedSubmissionsCount { get; set; }
    public int UpcomingDeadlinesCount { get; set; }
    public List<TeacherAssignedClassSubjectDto> AssignedClasses { get; set; } = new();
    public List<TeacherUpcomingDeadlineDto> UpcomingDeadlines { get; set; } = new();
}
