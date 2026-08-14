using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.StudentDTOs;
using Backend.Handlers.Student;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Student
{
    public class StudentDashboardHandlerTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IUserService> _mockUserService;
        private readonly StudentDashboardHandler _handler;

        public StudentDashboardHandlerTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _mockUserService = new Mock<IUserService>();

            _handler = new StudentDashboardHandler(_context, _mockUserService.Object, Mock.Of<ILogger<StudentDashboardHandler>>());
        }

        [Fact]
        public async Task HandleDashboardAsync_ShouldReturnDefaultData_WhenNoStudentFound()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((Guid.Empty, "", new List<string>()));

            // Act
            var result = await _handler.HandleDashboardAsync(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<StudentDashboardResponseDto>(okResult.Value);
            Assert.Equal("Student User", dto.StudentName);
            Assert.Equal(0, dto.EnrolledClassesCount);
        }

        [Fact]
        public async Task HandleDashboardAsync_ShouldCalculateMetrics_WhenStudentExists()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var student = new User { Id = studentId, FullName = "Alice Student", Email = "alice@test.com", PasswordHash = "h", Role = UserRole.Student };
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Physics", Code = "PHY" };
            var enrollment = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = studentId, ClassId = cls.Id };

            var teacher = new User { Id = Guid.NewGuid(), FullName = "Prof Science", Email = "prof@test.com", PasswordHash = "h", Role = UserRole.Teacher };
            var assignment = new Assignment { Id = Guid.NewGuid(), Title = "Physics Lab", Description = "D", ClassId = cls.Id, SubjectId = sub.Id, TeacherId = teacher.Id, Deadline = DateTime.UtcNow.AddDays(2), MaxMarks = 100 };
            var submission = new Submission { Id = Guid.NewGuid(), AssignmentId = assignment.Id, StudentId = studentId, SubmittedAt = DateTime.UtcNow, Marks = 85, GradeGiver = teacher };

            _context.Users.AddRange(student, teacher);
            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.StudentEnrollments.Add(enrollment);
            _context.Assignments.Add(assignment);
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((studentId, student.Email, new List<string> { "Student" }));

            // Act
            var result = await _handler.HandleDashboardAsync(studentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<StudentDashboardResponseDto>(okResult.Value);
            Assert.Equal("Alice Student", dto.StudentName);
            Assert.Equal(1, dto.EnrolledClassesCount);
            Assert.Equal(1, dto.CompletedAssignmentsCount);
            Assert.Equal(85, dto.AverageGrade);
        }
    }
}
