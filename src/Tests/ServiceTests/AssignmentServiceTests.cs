using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.StudentDTOs;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class AssignmentServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IUserService> _mockUserService;
        private readonly DefaultHttpContext _httpContext;
        private readonly AssignmentService _service;
        private readonly Guid _currentTeacherId;

        public AssignmentServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _currentTeacherId = Guid.NewGuid();
            (_mockHttpContextAccessor, _httpContext) = MockHelper.CreateMockHttpContext(userId: _currentTeacherId, role: "Teacher");
            _mockUserService = new Mock<IUserService>();

            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_currentTeacherId, "teacher@test.com", new List<string> { "Teacher" }));

            _service = new AssignmentService(_context, _mockHttpContextAccessor.Object, _mockUserService.Object);
        }

        [Fact]
        public async Task GetAllAssignmentsAsync_ShouldReturnAllAssignments()
        {
            // Arrange
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Math", Code = "M101" };
            var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", PasswordHash = "h", Role = UserRole.Teacher };

            var a1 = new Assignment { Id = Guid.NewGuid(), Title = "A1", Description = "D1", ClassId = cls.Id, SubjectId = sub.Id, TeacherId = teacher.Id, Class = cls, Subject = sub, Teacher = teacher, Deadline = DateTime.UtcNow.AddDays(2) };
            var a2 = new Assignment { Id = Guid.NewGuid(), Title = "A2", Description = "D2", ClassId = cls.Id, SubjectId = sub.Id, TeacherId = teacher.Id, Class = cls, Subject = sub, Teacher = teacher, Deadline = DateTime.UtcNow.AddDays(3) };

            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.Users.Add(teacher);
            _context.Assignments.AddRange(a1, a2);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllAssignmentsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAssignmentByIdAsync_ShouldReturnAssignment_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var assignment = new Assignment { Id = id, Title = "A1", Description = "D1", ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid(), TeacherId = Guid.NewGuid(), Deadline = DateTime.UtcNow.AddDays(1) };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAssignmentByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("A1", result.Title);
        }

        [Fact]
        public async Task CreateAssignmentAsync_ShouldCreateAndReturnResponseDto()
        {
            // Arrange
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();

            var cls = new Class { Id = classId, Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = subjectId, Name = "Math", Code = "M101" };
            var teacher = new User { Id = _currentTeacherId, FullName = "Current Teacher", Email = "teacher@test.com", PasswordHash = "h", Role = UserRole.Teacher };

            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.Users.Add(teacher);
            await _context.SaveChangesAsync();

            var dto = new AssignmentCreateDto
            {
                Title = "New Homework",
                Description = "Complete exercises 1 to 5",
                ClassId = classId,
                SubjectId = subjectId,
                Deadline = DateTime.UtcNow.AddDays(7),
                MaxMarks = 100,
                Status = AssignmentStatus.Published,
                AllowLateSubmission = true,
                AllowResubmission = false
            };

            // Act
            var result = await _service.CreateAssignmentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto.Title, result.Title);
            Assert.Equal("Class 10", result.ClassName);
            Assert.Equal("Math", result.SubjectName);
            Assert.Equal("Current Teacher", result.TeacherName);

            var dbItem = await _context.Assignments.FindAsync(result.Id);
            Assert.NotNull(dbItem);
            Assert.Equal(_currentTeacherId, dbItem.TeacherId);
        }

        [Fact]
        public async Task UpdateAssignmentAsync_ShouldThrowNotFound_WhenAssignmentDoesNotExist()
        {
            // Arrange
            var dto = new AssignmentUpdateDto { Title = "Updated" };

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAssignmentAsync(Guid.NewGuid(), dto));
        }

        [Fact]
        public async Task UpdateAssignmentAsync_ShouldThrowForbidden_WhenUserIsNotOwner()
        {
            // Arrange
            var otherTeacherId = Guid.NewGuid();
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Original",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = otherTeacherId,
                Deadline = DateTime.UtcNow.AddDays(2)
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var dto = new AssignmentUpdateDto { Title = "Updated Title" };

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _service.UpdateAssignmentAsync(assignment.Id, dto));
        }

        [Fact]
        public async Task UpdateAssignmentAsync_ShouldUpdateFields_WhenOwner()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Original",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = _currentTeacherId,
                Deadline = DateTime.UtcNow.AddDays(2),
                MaxMarks = 50,
                Status = AssignmentStatus.Draft
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            var dto = new AssignmentUpdateDto
            {
                Title = "Updated Title",
                MaxMarks = 100,
                Status = AssignmentStatus.Published
            };

            // Act
            var result = await _service.UpdateAssignmentAsync(assignment.Id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal(100, result.MaxMarks);
        }

        [Fact]
        public async Task DeleteAssignmentAsync_ShouldThrowForbidden_WhenNotOwner()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Original",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = Guid.NewGuid(),
                Deadline = DateTime.UtcNow.AddDays(2)
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _service.DeleteAssignmentAsync(assignment.Id));
        }

        [Fact]
        public async Task DeleteAssignmentAsync_ShouldRemoveAssignment_WhenOwner()
        {
            // Arrange
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "To Delete",
                Description = "Desc",
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid(),
                TeacherId = _currentTeacherId,
                Deadline = DateTime.UtcNow.AddDays(2)
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteAssignmentAsync(assignment.Id);

            // Assert
            var dbItem = await _context.Assignments.FindAsync(assignment.Id);
            Assert.Null(dbItem);
        }

        [Fact]
        public async Task GetAssignmentSummaryAsync_ShouldCalculateSummaryMetrics()
        {
            // Arrange
            _context.Assignments.AddRange(
                new Assignment { Id = Guid.NewGuid(), Title = "A1", Description = "D", ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid(), TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(1), Status = AssignmentStatus.Published },
                new Assignment { Id = Guid.NewGuid(), Title = "A2", Description = "D", ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid(), TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(5), Status = AssignmentStatus.Published },
                new Assignment { Id = Guid.NewGuid(), Title = "A3", Description = "D", ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid(), TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(2), Status = AssignmentStatus.Draft }
            );
            await _context.SaveChangesAsync();

            // Act
            var summary = await _service.GetAssignmentSummaryAsync();

            // Assert
            Assert.Equal(3, summary.TotalAssignments);
            Assert.Equal(3, summary.ActiveAssignments);
            Assert.Equal(1, summary.DraftAssignments);
            Assert.Equal(2, summary.DueSoonAssignments); // Within 3 days: A1 (1 day) and A3 (2 days)
        }

        [Fact]
        public async Task GetUpcomingDeadlines_ShouldReturnDeadlinesWithin3Days()
        {
            // Arrange
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 1", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Subject 1", Code = "S1" };
            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);

            _context.Assignments.AddRange(
                new Assignment { Id = Guid.NewGuid(), Title = "Due Soon", Description = "D", ClassId = cls.Id, SubjectId = sub.Id, TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(1), Class = cls, Subject = sub },
                new Assignment { Id = Guid.NewGuid(), Title = "Due Later", Description = "D", ClassId = cls.Id, SubjectId = sub.Id, TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(10), Class = cls, Subject = sub }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUpcomingDeadlines(_currentTeacherId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Due Soon", result[0].Title);
        }

        [Fact]
        public async Task GetAssignmentsForStudentPagedAsync_ShouldCalculateStatusAndFilter()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();

            var student = new User { Id = studentId, FullName = "Student One", Email = "s1@test.com", PasswordHash = "h", Role = UserRole.Student };
            var teacher = new User { Id = _currentTeacherId, FullName = "Teacher", Email = "t@test.com", PasswordHash = "h", Role = UserRole.Teacher };
            var cls = new Class { Id = classId, Name = "Math Class", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = subjectId, Name = "Algebra", Code = "ALG" };
            var enrollment = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = studentId, ClassId = classId };

            var a1 = new Assignment { Id = Guid.NewGuid(), Title = "Pending Assignment", Description = "D", ClassId = classId, SubjectId = subjectId, TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(5), CreatedAt = DateTime.UtcNow };
            var a2 = new Assignment { Id = Guid.NewGuid(), Title = "Submitted Assignment", Description = "D", ClassId = classId, SubjectId = subjectId, TeacherId = _currentTeacherId, Deadline = DateTime.UtcNow.AddDays(5), CreatedAt = DateTime.UtcNow };

            var sub2 = new Submission { Id = Guid.NewGuid(), AssignmentId = a2.Id, StudentId = studentId, SubmittedAt = DateTime.UtcNow, Status = SubmissionStatus.Submitted };

            _context.Users.AddRange(student, teacher);
            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.StudentEnrollments.Add(enrollment);
            _context.Assignments.AddRange(a1, a2);
            _context.Submissions.Add(sub2);
            await _context.SaveChangesAsync();

            var filter = new StudentAssignmentFilterDto { StatusFilter = "Submitted" };

            // Act
            var result = await _service.GetAssignmentsForStudentPagedAsync(studentId, filter);

            // Assert
            Assert.Equal(1, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Single(items);
            Assert.Equal("Submitted Assignment", items[0].Title);
            Assert.Equal("Submitted", items[0].Status);
        }

        [Fact]
        public async Task GetAssignmentDetailForStudentAsync_ShouldReturnDetailsAndExistingSubmission()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var assignmentId = Guid.NewGuid();

            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "History", Code = "HIS" };
            var teacher = new User { Id = _currentTeacherId, FullName = "Prof Hist", Email = "h@test.com", PasswordHash = "h", Role = UserRole.Teacher };

            var assignment = new Assignment
            {
                Id = assignmentId,
                Title = "WW2 Essay",
                Description = "Write 500 words",
                ClassId = cls.Id,
                SubjectId = sub.Id,
                TeacherId = teacher.Id,
                Deadline = DateTime.UtcNow.AddDays(3),
                MaxMarks = 100,
                Class = cls,
                Subject = sub,
                Teacher = teacher
            };

            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                StudentId = studentId,
                SubmissionText = "My history essay",
                SubmittedAt = DateTime.UtcNow,
                Marks = 90,
                Feedback = "Great essay",
                GradeGiver = teacher
            };

            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.Users.Add(teacher);
            _context.Assignments.Add(assignment);
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAssignmentDetailForStudentAsync(studentId, assignmentId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WW2 Essay", result.Title);
            Assert.Equal("Graded", result.Status);
            Assert.NotNull(result.ExistingSubmission);
            Assert.Equal("My history essay", result.ExistingSubmission.SubmissionText);
            Assert.Equal(90, result.ExistingSubmission.Marks);
        }
    }
}
