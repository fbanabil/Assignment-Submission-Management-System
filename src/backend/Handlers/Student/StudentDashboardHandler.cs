using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentDTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Backend.Handlers.Student
{
    public class StudentDashboardHandler
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;
        private readonly ILogger<StudentDashboardHandler> _logger;

        public StudentDashboardHandler(AppDbContext context, IUserService userService, ILogger<StudentDashboardHandler> logger)
        {
            _context = context;
            _userService = userService;
            _logger = logger;
        }

        public async Task<IActionResult> HandleDashboardAsync(Guid? requestedStudentId)
        {
            _logger.LogInformation("StudentDashboardHandler: Fetching dashboard data for requested StudentId:{StudentId}", requestedStudentId);
            Guid studentId = Guid.Empty;

            if (requestedStudentId.HasValue && requestedStudentId.Value != Guid.Empty)
            {
                studentId = requestedStudentId.Value;
            }
            else
            {
                var claimsInfo = await _userService.GetUserIdAndEmailFromClaims();
                studentId = claimsInfo.UserId;
            }

            if (studentId == Guid.Empty)
            {
                var fallbackUser = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Student);
                if (fallbackUser != null) studentId = fallbackUser.Id;
            }

            var studentUser = await _context.Users.FindAsync(studentId);
            if (studentUser == null)
            {
                _logger.LogWarning("StudentDashboardHandler: Student user not found for ID:{StudentId}", studentId);
                return new OkObjectResult(new StudentDashboardResponseDto
                {
                    StudentName = "Student User",
                    StudentEmail = "student@example.com",
                    EnrolledClassesCount = 0,
                    PendingAssignmentsCount = 0,
                    CompletedAssignmentsCount = 0,
                    AverageGrade = 0,
                    AssignmentsDueSoon = new List<StudentAssignmentDueDto>(),
                    RecentGradesFeedback = new List<StudentRecentGradeDto>(),
                    DataSource = "Server API",
                    FetchedAt = DateTime.UtcNow
                });
            }

            // Enrolled class IDs
            var enrolledClassIds = await _context.StudentEnrollments
                .Where(e => e.StudentId == studentId)
                .Select(e => e.ClassId)
                .ToListAsync();

            int enrolledClassesCount = enrolledClassIds.Count;

            // Get assignments for enrolled classes
            var allAssignments = await _context.Assignments
                .Include(a => a.Class)
                .Include(a => a.Subject)
                .Where(a => enrolledClassIds.Contains(a.ClassId))
                .ToListAsync();

            // Student's submissions
            var submissions = await _context.Submissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Subject)
                .Include(s => s.GradeGiver)
                .Where(s => s.StudentId == studentId)
                .ToListAsync();

            var submittedAssignmentIds = submissions.Select(s => s.AssignmentId).ToHashSet();

            // Assignments due soon
            var dueSoonList = allAssignments
                .Where(a => !submittedAssignmentIds.Contains(a.Id))
                .OrderBy(a => a.Deadline)
                .Take(10)
                .Select(a => new StudentAssignmentDueDto
                {
                    AssignmentId = a.Id,
                    Title = a.Title,
                    SubjectName = a.Subject?.Name ?? "General",
                    SubjectCode = a.Subject?.Code ?? "GEN",
                    ClassName = a.Class?.Name ?? "Class",
                    DueDate = a.Deadline,
                    MaxMarks = a.MaxMarks,
                    Status = a.Deadline < DateTime.UtcNow ? "Overdue" : "Pending"
                })
                .ToList();

            // Recent grades & feedback
            var recentGrades = submissions
                .Where(s => s.Marks.HasValue || !string.IsNullOrWhiteSpace(s.Feedback))
                .OrderByDescending(s => s.GradedAt ?? s.SubmittedAt)
                .Take(10)
                .Select(s => new StudentRecentGradeDto
                {
                    SubmissionId = s.Id,
                    AssignmentTitle = s.Assignment?.Title ?? "Assignment",
                    SubjectName = s.Assignment?.Subject?.Name ?? "Subject",
                    SubjectCode = s.Assignment?.Subject?.Code ?? "SUB",
                    SubmittedAt = s.SubmittedAt,
                    GradedAt = s.GradedAt,
                    Grade = s.Marks,
                    MaxMarks = s.Assignment?.MaxMarks ?? 100,
                    Feedback = s.Feedback ?? "No feedback provided.",
                    GradedByTeacherName = s.GradeGiver?.FullName ?? "Teacher"
                })
                .ToList();

            int pendingCount = allAssignments.Count(a => !submittedAssignmentIds.Contains(a.Id));
            int completedCount = submissions.Count;

            var gradedSubmissions = submissions.Where(s => s.Marks.HasValue).ToList();
            double avgGrade = gradedSubmissions.Count > 0
                ? Math.Round(gradedSubmissions.Average(s => s.Marks!.Value), 1)
                : 0;

            _logger.LogInformation("StudentDashboardHandler: Dashboard compiled for {StudentName} - Classes:{EnrolledClassesCount}, Pending:{PendingCount}, AvgGrade:{AvgGrade}",
                studentUser.FullName, enrolledClassesCount, pendingCount, avgGrade);

            var response = new StudentDashboardResponseDto
            {
                StudentName = studentUser.FullName,
                StudentEmail = studentUser.Email,
                EnrolledClassesCount = enrolledClassesCount,
                PendingAssignmentsCount = pendingCount,
                CompletedAssignmentsCount = completedCount,
                AverageGrade = avgGrade,
                AssignmentsDueSoon = dueSoonList,
                RecentGradesFeedback = recentGrades,
                DataSource = "Server API",
                FetchedAt = DateTime.UtcNow
            };

            return new OkObjectResult(response);
        }
    }
}
