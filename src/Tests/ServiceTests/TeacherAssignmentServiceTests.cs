using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.Middlewares;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class TeacherAssignmentServiceTests
    {
        private readonly AppDbContext _context;
        private readonly TeacherAssignmentService _service;

        public TeacherAssignmentServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _service = new TeacherAssignmentService(_context, Mock.Of<ILogger<TeacherAssignmentService>>());
        }

        [Fact]
        public async Task GetAllTeacherAssignmentsAsync_ShouldReturnAllWithIncludes()
        {
            // Arrange
            var teacher = new User { Id = Guid.NewGuid(), FullName = "Prof Smith", Email = "smith@test.com", PasswordHash = "h", Role = UserRole.Teacher };
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Math", Code = "M101" };
            var cs = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls.Id, SubjectId = sub.Id, Class = cls, Subject = sub };
            var ta = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacher.Id, ClassSubjectId = cs.Id, Teacher = teacher, ClassSubject = cs };

            _context.Users.Add(teacher);
            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.ClassSubjects.Add(cs);
            _context.TeacherAssignments.Add(ta);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllTeacherAssignmentsAsync();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.Equal("Prof Smith", item.Teacher.FullName);
            Assert.NotNull(item.ClassSubject);
        }

        [Fact]
        public async Task GetTeacherAssignmentByIdAsync_ShouldReturnItem_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var ta = new TeacherAssignment { Id = id, TeacherId = Guid.NewGuid(), ClassSubjectId = Guid.NewGuid() };
            _context.TeacherAssignments.Add(ta);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetTeacherAssignmentByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task CreateTeacherAssignmentAsync_ShouldCreateWithClassSubjectId()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var csId = Guid.NewGuid();
            var dto = new TeacherAssignmentCreateDto
            {
                TeacherId = teacherId,
                ClassSubjectId = csId
            };

            // Act
            var result = await _service.CreateTeacherAssignmentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(teacherId, result.TeacherId);
            Assert.Equal(csId, result.ClassSubjectId);

            var dbTa = await _context.TeacherAssignments.FindAsync(result.Id);
            Assert.NotNull(dbTa);
        }

        [Fact]
        public async Task CreateTeacherAssignmentAsync_ShouldAutoCreateClassSubject_WhenClassSubjectIdEmpty()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();

            var dto = new TeacherAssignmentCreateDto
            {
                TeacherId = teacherId,
                ClassSubjectId = Guid.Empty,
                ClassId = classId,
                SubjectId = subjectId
            };

            // Act
            var result = await _service.CreateTeacherAssignmentAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.ClassSubjectId);

            var createdCs = await _context.ClassSubjects.FirstOrDefaultAsync(cs => cs.ClassId == classId && cs.SubjectId == subjectId);
            Assert.NotNull(createdCs);
            Assert.Equal(createdCs.Id, result.ClassSubjectId);
        }

        [Fact]
        public async Task CreateTeacherAssignmentAsync_ShouldThrowBadRequest_WhenAssignmentAlreadyExists()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();

            var existingCs = new ClassSubject { Id = Guid.NewGuid(), ClassId = classId, SubjectId = subjectId };
            _context.ClassSubjects.Add(existingCs);
            _context.TeacherAssignments.Add(new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacherId, ClassSubjectId = existingCs.Id });
            await _context.SaveChangesAsync();

            var dto = new TeacherAssignmentCreateDto
            {
                TeacherId = teacherId,
                ClassSubjectId = Guid.Empty,
                ClassId = classId,
                SubjectId = subjectId
            };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateTeacherAssignmentAsync(dto));
        }

        [Fact]
        public async Task DeleteTeacherAssignmentAsync_ShouldRemoveAssignment()
        {
            // Arrange
            var id = Guid.NewGuid();
            _context.TeacherAssignments.Add(new TeacherAssignment { Id = id, TeacherId = Guid.NewGuid(), ClassSubjectId = Guid.NewGuid() });
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteTeacherAssignmentAsync(id);

            // Assert
            var dbTa = await _context.TeacherAssignments.FindAsync(id);
            Assert.Null(dbTa);
        }

        [Fact]
        public async Task GetAssignedClasses_ShouldReturnAssignedClassesWithStudentCount()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Science", Code = "SCI101" };
            var cs = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls.Id, SubjectId = sub.Id, Class = cls, Subject = sub };
            var ta = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacherId, ClassSubjectId = cs.Id, ClassSubject = cs };

            var student = new User { Id = Guid.NewGuid(), FullName = "Student A", Email = "sa@test.com", PasswordHash = "h", Role = UserRole.Student };
            var enrollment = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = cls.Id, Class = cls, Student = student };

            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.ClassSubjects.Add(cs);
            _context.TeacherAssignments.Add(ta);
            _context.Users.Add(student);
            _context.StudentEnrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAssignedClasses(teacherId);

            // Assert
            Assert.Single(result);
            Assert.Equal("Class 10", result[0].ClassName);
            Assert.Equal("Science", result[0].SubjectName);
            Assert.Equal(1, result[0].StudentCount);
        }

        [Fact]
        public async Task GetAssignedClassesPagedAsync_ShouldFilterAndPaginate()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var cls1 = new Class { Id = Guid.NewGuid(), Name = "Class Alpha", Section = "A", AcademicYear = "2026" };
            var cls2 = new Class { Id = Guid.NewGuid(), Name = "Class Beta", Section = "B", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Math", Code = "M101" };

            var cs1 = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls1.Id, SubjectId = sub.Id, Class = cls1, Subject = sub };
            var cs2 = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls2.Id, SubjectId = sub.Id, Class = cls2, Subject = sub };

            var ta1 = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacherId, ClassSubjectId = cs1.Id, ClassSubject = cs1 };
            var ta2 = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacherId, ClassSubjectId = cs2.Id, ClassSubject = cs2 };

            _context.Classes.AddRange(cls1, cls2);
            _context.Subjects.Add(sub);
            _context.ClassSubjects.AddRange(cs1, cs2);
            _context.TeacherAssignments.AddRange(ta1, ta2);
            await _context.SaveChangesAsync();

            var filter = new TeacherClassFilterDto
            {
                ClassName = "Alpha",
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _service.GetAssignedClassesPagedAsync(teacherId, filter);

            // Assert
            Assert.Equal(1, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Single(items);
            Assert.Equal("Class Alpha", items[0].ClassName);
        }
    }
}
