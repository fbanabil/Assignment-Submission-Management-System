using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.ClassSubjectDTOs;
using Backend.Middlewares;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class ClassSubjectServiceTests
    {
        private readonly AppDbContext _context;
        private readonly ClassSubjectService _service;

        public ClassSubjectServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _service = new ClassSubjectService(_context, Mock.Of<ILogger<ClassSubjectService>>());
        }

        [Fact]
        public async Task GetAllClassSubjectsAsync_ShouldReturnAllWithIncludes()
        {
            // Arrange
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Science", Code = "SCI101" };
            var cs = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls.Id, SubjectId = sub.Id, Class = cls, Subject = sub };

            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.ClassSubjects.Add(cs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllClassSubjectsAsync();

            // Assert
            Assert.Single(result);
            var item = result.First();
            Assert.NotNull(item.Class);
            Assert.NotNull(item.Subject);
            Assert.Equal("Class 10", item.Class.Name);
            Assert.Equal("Science", item.Subject.Name);
        }

        [Fact]
        public async Task GetClassSubjectByIdAsync_ShouldReturnItem_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cs = new ClassSubject { Id = id, ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid() };
            _context.ClassSubjects.Add(cs);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetClassSubjectByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
        }

        [Fact]
        public async Task GetClassSubjectByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetClassSubjectByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateClassSubjectAsync_ShouldAddAndReturnClassSubject_WhenUnique()
        {
            // Arrange
            var dto = new ClassSubjectCreateDto
            {
                ClassId = Guid.NewGuid(),
                SubjectId = Guid.NewGuid()
            };

            // Act
            var created = await _service.CreateClassSubjectAsync(dto);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(dto.ClassId, created.ClassId);
            Assert.Equal(dto.SubjectId, created.SubjectId);

            var dbItem = await _context.ClassSubjects.FindAsync(created.Id);
            Assert.NotNull(dbItem);
        }

        [Fact]
        public async Task CreateClassSubjectAsync_ShouldThrowBadRequestException_WhenDuplicateExists()
        {
            // Arrange
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            _context.ClassSubjects.Add(new ClassSubject { Id = Guid.NewGuid(), ClassId = classId, SubjectId = subjectId });
            await _context.SaveChangesAsync();

            var dto = new ClassSubjectCreateDto { ClassId = classId, SubjectId = subjectId };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateClassSubjectAsync(dto));
        }

        [Fact]
        public async Task DeleteClassSubjectAsync_ShouldDeleteClassSubjectAndDependentTeacherAssignments()
        {
            // Arrange
            var csId = Guid.NewGuid();
            var cs = new ClassSubject { Id = csId, ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid() };
            var ta1 = new TeacherAssignment { Id = Guid.NewGuid(), ClassSubjectId = csId, TeacherId = Guid.NewGuid() };
            var ta2 = new TeacherAssignment { Id = Guid.NewGuid(), ClassSubjectId = csId, TeacherId = Guid.NewGuid() };

            _context.ClassSubjects.Add(cs);
            _context.TeacherAssignments.AddRange(ta1, ta2);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteClassSubjectAsync(csId);

            // Assert
            var dbCs = await _context.ClassSubjects.FindAsync(csId);
            Assert.Null(dbCs);

            var dbTas = _context.TeacherAssignments.Where(ta => ta.ClassSubjectId == csId).ToList();
            Assert.Empty(dbTas);
        }

        [Fact]
        public async Task DeleteClassSubjectAsync_ShouldDoNothing_WhenNotExists()
        {
            // Act & Assert (Should not throw)
            await _service.DeleteClassSubjectAsync(Guid.NewGuid());
        }
    }
}
