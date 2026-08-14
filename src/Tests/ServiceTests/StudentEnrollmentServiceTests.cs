using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.Middlewares;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class StudentEnrollmentServiceTests
    {
        private readonly AppDbContext _context;
        private readonly StudentEnrollmentService _service;

        public StudentEnrollmentServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _service = new StudentEnrollmentService(_context, Mock.Of<ILogger<StudentEnrollmentService>>());
        }

        [Fact]
        public async Task GetAllStudentEnrollmentsAsync_ShouldReturnAllWithIncludes()
        {
            // Arrange
            var student = new User { Id = Guid.NewGuid(), FullName = "Student 1", Email = "s1@test.com", PasswordHash = "h", Role = UserRole.Student };
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var enrollment = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = cls.Id, Student = student, Class = cls, EnrolledAt = DateTime.UtcNow };

            _context.Users.Add(student);
            _context.Classes.Add(cls);
            _context.StudentEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllStudentEnrollmentsAsync();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Student 1", item.Student.FullName);
            Assert.Equal("Class 10", item.Class.Name);
        }

        [Fact]
        public async Task GetStudentEnrollmentByIdAsync_ShouldReturnItem_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var enrollment = new StudentEnrollment { Id = id, StudentId = Guid.NewGuid(), ClassId = Guid.NewGuid(), EnrolledAt = DateTime.UtcNow };
            _context.StudentEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetStudentEnrollmentByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task CreateStudentEnrollmentAsync_ShouldThrowBadRequest_WhenUserNotFound()
        {
            // Arrange
            var dto = new StudentEnrollmentCreateDto { StudentEmail = "nonexistent@test.com", ClassId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateStudentEnrollmentAsync(dto));
        }

        [Fact]
        public async Task CreateStudentEnrollmentAsync_ShouldThrowBadRequest_WhenUserIsNotStudentRole()
        {
            // Arrange
            var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher 1", Email = "t1@test.com", PasswordHash = "h", Role = UserRole.Teacher };
            _context.Users.Add(teacher);
            await _context.SaveChangesAsync();

            var dto = new StudentEnrollmentCreateDto { StudentEmail = "t1@test.com", ClassId = Guid.NewGuid() };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateStudentEnrollmentAsync(dto));
        }

        [Fact]
        public async Task CreateStudentEnrollmentAsync_ShouldThrowBadRequest_WhenAlreadyEnrolled()
        {
            // Arrange
            var student = new User { Id = Guid.NewGuid(), FullName = "Student 1", Email = "s1@test.com", PasswordHash = "h", Role = UserRole.Student };
            var classId = Guid.NewGuid();
            _context.Users.Add(student);
            _context.StudentEnrollments.Add(new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = classId, EnrolledAt = DateTime.UtcNow });
            await _context.SaveChangesAsync();

            var dto = new StudentEnrollmentCreateDto { StudentEmail = "s1@test.com", ClassId = classId };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateStudentEnrollmentAsync(dto));
        }

        [Fact]
        public async Task CreateStudentEnrollmentAsync_ShouldCreateEnrollment_WhenValid()
        {
            // Arrange
            var student = new User { Id = Guid.NewGuid(), FullName = "Student 1", Email = "s1@test.com", PasswordHash = "h", Role = UserRole.Student };
            var classId = Guid.NewGuid();
            _context.Users.Add(student);
            await _context.SaveChangesAsync();

            var dto = new StudentEnrollmentCreateDto { StudentEmail = "s1@test.com", ClassId = classId };

            // Act
            var result = await _service.CreateStudentEnrollmentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(student.Id, result.StudentId);
            Assert.Equal(classId, result.ClassId);

            var dbEnrollment = await _context.StudentEnrollments.FindAsync(result.Id);
            Assert.NotNull(dbEnrollment);
        }

        [Fact]
        public async Task DeleteStudentEnrollmentAsync_ShouldRemoveEnrollment()
        {
            // Arrange
            var id = Guid.NewGuid();
            var enrollment = new StudentEnrollment { Id = id, StudentId = Guid.NewGuid(), ClassId = Guid.NewGuid(), EnrolledAt = DateTime.UtcNow };
            _context.StudentEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteStudentEnrollmentAsync(id);

            // Assert
            var dbEnrollment = await _context.StudentEnrollments.FindAsync(id);
            Assert.Null(dbEnrollment);
        }

        [Fact]
        public async Task GetEnrolledClassIdsAsync_ShouldReturnEnrolledClassIds()
        {
            // Arrange
            var studentId = Guid.NewGuid();
            var c1 = Guid.NewGuid();
            var c2 = Guid.NewGuid();
            _context.StudentEnrollments.AddRange(
                new StudentEnrollment { Id = Guid.NewGuid(), StudentId = studentId, ClassId = c1, EnrolledAt = DateTime.UtcNow },
                new StudentEnrollment { Id = Guid.NewGuid(), StudentId = studentId, ClassId = c2, EnrolledAt = DateTime.UtcNow },
                new StudentEnrollment { Id = Guid.NewGuid(), StudentId = Guid.NewGuid(), ClassId = Guid.NewGuid(), EnrolledAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetEnrolledClassIdsAsync(studentId);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(c1, result);
            Assert.Contains(c2, result);
        }

        [Fact]
        public async Task GetStudentEnrollmentsForTeacherAsync_ShouldOnlyReturnClassesTaughtByTeacher()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var taughtClass = new Class { Id = Guid.NewGuid(), Name = "Taught Class", Section = "A", AcademicYear = "2026" };
            var untaughtClass = new Class { Id = Guid.NewGuid(), Name = "Other Class", Section = "B", AcademicYear = "2026" };
            var subject = new Subject { Id = Guid.NewGuid(), Name = "Subject 1", Code = "SUB1" };
            var cs = new ClassSubject { Id = Guid.NewGuid(), ClassId = taughtClass.Id, SubjectId = subject.Id, Class = taughtClass, Subject = subject };
            var ta = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacherId, ClassSubjectId = cs.Id, ClassSubject = cs };

            var student1 = new User { Id = Guid.NewGuid(), FullName = "Student One", Email = "s1@test.com", PasswordHash = "h", Role = UserRole.Student };
            var student2 = new User { Id = Guid.NewGuid(), FullName = "Student Two", Email = "s2@test.com", PasswordHash = "h", Role = UserRole.Student };

            var enrollment1 = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student1.Id, ClassId = taughtClass.Id, Student = student1, Class = taughtClass, EnrolledAt = DateTime.UtcNow };
            var enrollment2 = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student2.Id, ClassId = untaughtClass.Id, Student = student2, Class = untaughtClass, EnrolledAt = DateTime.UtcNow };

            _context.Classes.AddRange(taughtClass, untaughtClass);
            _context.Subjects.Add(subject);
            _context.ClassSubjects.Add(cs);
            _context.TeacherAssignments.Add(ta);
            _context.Users.AddRange(student1, student2);
            _context.StudentEnrollments.AddRange(enrollment1, enrollment2);
            await _context.SaveChangesAsync();

            var filter = new StudentEnrollmentFilterDto();

            // Act
            var result = await _service.GetStudentEnrollmentsForTeacherAsync(teacherId, filter);

            // Assert
            Assert.Equal(1, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Equal("Student One", items[0].StudentName);
            Assert.Equal("Taught Class", items[0].ClassName);
        }
    }
}
