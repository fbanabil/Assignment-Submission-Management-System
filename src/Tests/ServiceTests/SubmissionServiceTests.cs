using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.StudentDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class SubmissionServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly SubmissionService _service;
        private readonly Guid _currentStudentId;
        private readonly Guid _currentTeacherId;

        public SubmissionServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _mockUserService = new Mock<IUserService>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();

            _currentStudentId = Guid.NewGuid();
            _currentTeacherId = Guid.NewGuid();

            _mockEnvironment.Setup(e => e.WebRootPath).Returns(Path.GetTempPath());

            _service = new SubmissionService(_context, _mockUserService.Object, _mockEnvironment.Object, Mock.Of<ILogger<SubmissionService>>());
        }

        [Fact]
        public async Task GetAllSubmissionsAsync_ShouldReturnAllSubmissions()
        {
            // Arrange
            var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", PasswordHash = "h", Role = UserRole.Student };
            var assignment = new Assignment { Id = Guid.NewGuid(), Title = "A1", Description = "D1", ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid(), TeacherId = Guid.NewGuid(), Deadline = DateTime.UtcNow.AddDays(1) };
            var sub = new Submission { Id = Guid.NewGuid(), AssignmentId = assignment.Id, StudentId = student.Id, Student = student, Assignment = assignment, SubmittedAt = DateTime.UtcNow };

            _context.Users.Add(student);
            _context.Assignments.Add(assignment);
            _context.Submissions.Add(sub);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllSubmissionsAsync();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetSubmissionByIdAsync_ShouldReturnSubmission_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var sub = new Submission { Id = id, AssignmentId = Guid.NewGuid(), StudentId = Guid.NewGuid(), SubmittedAt = DateTime.UtcNow };
            _context.Submissions.Add(sub);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetSubmissionByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task CreateSubmissionAsync_ShouldThrowForbidden_WhenNotStudent()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentTeacherId, "teacher@test.com", new List<string> { "Teacher" }));

            var dto = new SubmissionCreateDto { AssignmentId = Guid.NewGuid(), StudentId = _currentStudentId };

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _service.CreateSubmissionAsync(dto));
        }

        [Fact]
        public async Task CreateSubmissionAsync_ShouldCreateSubmission_WhenStudent()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentStudentId, "student@test.com", new List<string> { "Student" }));

            var dto = new SubmissionCreateDto
            {
                AssignmentId = Guid.NewGuid(),
                StudentId = _currentStudentId,
                SubmissionText = "My submission content"
            };

            // Act
            var created = await _service.CreateSubmissionAsync(dto);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(dto.SubmissionText, created.SubmissionText);
            Assert.Equal(SubmissionStatus.Submitted, created.Status);

            var dbSub = await _context.Submissions.FindAsync(created.Id);
            Assert.NotNull(dbSub);
        }

        [Fact]
        public async Task UpdateSubmissionAsync_ShouldThrowForbidden_WhenNotOwner()
        {
            // Arrange
            var otherStudentId = Guid.NewGuid();
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentStudentId, "student@test.com", new List<string> { "Student" }));

            var sub = new Submission { Id = Guid.NewGuid(), AssignmentId = Guid.NewGuid(), StudentId = otherStudentId, SubmittedAt = DateTime.UtcNow };
            _context.Submissions.Add(sub);
            await _context.SaveChangesAsync();

            var dto = new SubmissionUpdateDto { SubmissionText = "Updated text" };

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateSubmissionAsync(sub.Id, dto));
        }

        [Fact]
        public async Task UpdateSubmissionAsync_ShouldUpdate_WhenOwner()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentStudentId, "student@test.com", new List<string> { "Student" }));

            var sub = new Submission { Id = Guid.NewGuid(), AssignmentId = Guid.NewGuid(), StudentId = _currentStudentId, SubmissionText = "Old", SubmittedAt = DateTime.UtcNow };
            _context.Submissions.Add(sub);
            await _context.SaveChangesAsync();

            var dto = new SubmissionUpdateDto { SubmissionText = "New submission text", FileUrl = "http://files.com/doc.pdf" };

            // Act
            await _service.UpdateSubmissionAsync(sub.Id, dto);

            // Assert
            var updated = await _context.Submissions.FindAsync(sub.Id);
            Assert.NotNull(updated);
            Assert.Equal("New submission text", updated.SubmissionText);
            Assert.Equal("http://files.com/doc.pdf", updated.FileUrl);
        }

        [Fact]
        public async Task GradeSubmissionAsync_ShouldThrowForbidden_WhenNotTeacher()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentStudentId, "student@test.com", new List<string> { "Student" }));

            var sub = new Submission { Id = Guid.NewGuid(), AssignmentId = Guid.NewGuid(), StudentId = Guid.NewGuid(), SubmittedAt = DateTime.UtcNow };
            _context.Submissions.Add(sub);
            await _context.SaveChangesAsync();

            var dto = new GradeDto { SubmissionId = sub.Id, Marks = 85, Feedback = "Good job" };

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _service.GradeSubmissionAsync(dto, _currentStudentId));
        }

        [Fact]
        public async Task GradeSubmissionAsync_ShouldUpdateMarksAndStatus_WhenTeacher()
        {
            // Arrange
            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentTeacherId, "teacher@test.com", new List<string> { "Teacher" }));

            var sub = new Submission { Id = Guid.NewGuid(), AssignmentId = Guid.NewGuid(), StudentId = Guid.NewGuid(), SubmittedAt = DateTime.UtcNow, Status = SubmissionStatus.Submitted };
            _context.Submissions.Add(sub);
            await _context.SaveChangesAsync();

            var dto = new GradeDto { SubmissionId = sub.Id, Marks = 95, Feedback = "Outstanding!" };

            // Act
            await _service.GradeSubmissionAsync(dto, _currentTeacherId);

            // Assert
            var graded = await _context.Submissions.FindAsync(sub.Id);
            Assert.NotNull(graded);
            Assert.Equal(95, graded.Marks);
            Assert.Equal("Outstanding!", graded.Feedback);
            Assert.Equal(_currentTeacherId, graded.GradedBy);
            Assert.Equal(SubmissionStatus.Graded, graded.Status);
        }

        [Fact]
        public async Task GetSubmissionSummaryAsync_ShouldCalculateMetrics()
        {
            // Arrange
            _context.Submissions.AddRange(
                new Submission { Id = Guid.NewGuid(), AssignmentId = Guid.NewGuid(), StudentId = Guid.NewGuid(), SubmittedAt = DateTime.UtcNow, Status = SubmissionStatus.Submitted },
                new Submission { Id = Guid.NewGuid(), AssignmentId = Guid.NewGuid(), StudentId = Guid.NewGuid(), SubmittedAt = DateTime.UtcNow, Status = SubmissionStatus.Graded, Marks = 80 }
            );
            await _context.SaveChangesAsync();

            // Act
            var summary = await _service.GetSubmissionSummaryAsync();

            // Assert
            Assert.Equal(2, summary.TotalSubmissions);
            Assert.Equal(2, summary.SubmittedToday);
            Assert.Equal(1, summary.PendingReview);
            Assert.Equal(1, summary.GradedSubmissions);
        }

        [Fact]
        public async Task CreateStudentSubmissionAsync_ShouldThrowBadRequest_WhenAssignmentNotFound()
        {
            // Arrange
            var dto = new StudentSubmissionCreateDto { AssignmentId = Guid.NewGuid(), SubmissionText = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateStudentSubmissionAsync(_currentStudentId, dto));
        }

        [Fact]
        public async Task CreateStudentSubmissionAsync_ShouldThrowBadRequest_WhenOverdueAndLateNotAllowed()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Past Due",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(-2),
                AllowLateSubmission = false
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var dto = new StudentSubmissionCreateDto { AssignmentId = assignment.Id, SubmissionText = "Late attempt" };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateStudentSubmissionAsync(_currentStudentId, dto));
        }

        [Fact]
        public async Task CreateStudentSubmissionAsync_ShouldThrowBadRequest_WhenResubmissionDisabled()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "No Resubmit",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(2),
                AllowResubmission = false
            };
            var existingSub = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment.Id,
                StudentId = _currentStudentId,
                SubmissionText = "First submission",
                SubmittedAt = DateTime.UtcNow
            };
            _context.Assignments.Add(assignment);
            _context.Submissions.Add(existingSub);
            await _context.SaveChangesAsync();

            var dto = new StudentSubmissionCreateDto { AssignmentId = assignment.Id, SubmissionText = "Second submission" };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateStudentSubmissionAsync(_currentStudentId, dto));
        }

        [Fact]
        public async Task CreateStudentSubmissionAsync_ShouldCreateNewSubmission_WhenValid()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Math Homework",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(5),
                AllowResubmission = true
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var dto = new StudentSubmissionCreateDto
            {
                AssignmentId = assignment.Id,
                SubmissionText = "Here is my work",
                FileUrl = "/assignments/homework.pdf"
            };

            // Act
            var result = await _service.CreateStudentSubmissionAsync(_currentStudentId, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Here is my work", result.SubmissionText);
            Assert.Equal("/assignments/homework.pdf", result.FileUrl);

            var dbSub = await _context.Submissions.FindAsync(result.SubmissionId);
            Assert.NotNull(dbSub);
        }

        [Fact]
        public async Task UploadAssignmentFileAsync_ShouldUploadFileAndReturnPath()
        {
            // Arrange
            var tempFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempFolder);

            try
            {
                var content = "This is test file content";
                var bytes = Encoding.UTF8.GetBytes(content);
                var stream = new MemoryStream(bytes);

                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.FileName).Returns("assignment1.pdf");
                mockFile.Setup(f => f.Length).Returns(bytes.Length);
                mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns((Stream targetStream, CancellationToken token) => stream.CopyToAsync(targetStream, token));

                // Act
                var result = await _service.UploadAssignmentFileAsync(mockFile.Object, tempFolder);

                // Assert
                Assert.NotNull(result);
                Assert.Equal("assignment1.pdf", result.OriginalFileName);
                Assert.StartsWith("/assignments/", result.FilePath);
                Assert.Equal(bytes.Length, result.FileSize);
            }
            finally
            {
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }
            }
        }

        [Fact]
        public async Task UploadAssignmentFileAsync_ShouldThrowBadRequest_WhenFileIsEmpty()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.UploadAssignmentFileAsync(mockFile.Object, "F:/temp"));
        }

        [Fact]
        public async Task UnsubmitAssignmentAsync_ShouldRemoveSubmission_WhenAllowed()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Resubmission Allowed",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(2),
                AllowResubmission = true
            };
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment.Id,
                StudentId = _currentStudentId,
                SubmissionText = "My unsubmit submission",
                SubmittedAt = DateTime.UtcNow,
                Assignment = assignment
            };
            _context.Assignments.Add(assignment);
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Act
            await _service.UnsubmitAssignmentAsync(_currentStudentId, submission.Id);

            // Assert
            var dbSub = await _context.Submissions.FindAsync(submission.Id);
            Assert.Null(dbSub);
        }

        [Fact]
        public async Task UnsubmitAssignmentAsync_ShouldThrowBadRequest_WhenResubmissionDisabled()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Resubmission Disabled",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(2),
                AllowResubmission = false
            };
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment.Id,
                StudentId = _currentStudentId,
                SubmissionText = "My submission",
                SubmittedAt = DateTime.UtcNow,
                Assignment = assignment
            };
            _context.Assignments.Add(assignment);
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.UnsubmitAssignmentAsync(_currentStudentId, submission.Id));
        }
    }
}
