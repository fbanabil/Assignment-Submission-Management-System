using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.SubjectDTOs;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class SubjectServiceTests
    {
        private readonly AppDbContext _context;
        private readonly SubjectService _service;

        public SubjectServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _service = new SubjectService(_context);
        }

        [Fact]
        public async Task GetAllSubjectsAsync_ShouldReturnAllSubjects()
        {
            // Arrange
            _context.Subjects.AddRange(
                new Subject { Id = Guid.NewGuid(), Name = "English", Code = "ENG101" },
                new Subject { Id = Guid.NewGuid(), Name = "Math", Code = "MTH101" }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllSubjectsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSubjectByIdAsync_ShouldReturnSubject_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var subject = new Subject { Id = id, Name = "History", Code = "HIS101" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetSubjectByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("History", result.Name);
            Assert.Equal("HIS101", result.Code);
        }

        [Fact]
        public async Task GetSubjectByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetSubjectByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateSubjectAsync_ShouldAddAndReturnSubject()
        {
            // Arrange
            var dto = new SubjectCreateDto { Name = "Chemistry", Code = "CHEM101" };

            // Act
            var created = await _service.CreateSubjectAsync(dto);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(dto.Name, created.Name);
            Assert.Equal(dto.Code, created.Code);

            var dbSubject = await _context.Subjects.FindAsync(created.Id);
            Assert.NotNull(dbSubject);
        }

        [Fact]
        public async Task UpdateSubjectAsync_ShouldUpdateProperties_WhenExists()
        {
            // Arrange
            var subject = new Subject { Id = Guid.NewGuid(), Name = "Physics Old", Code = "PHY100" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            var updateDto = new SubjectUpdateDto { Name = "Physics New", Code = "PHY101" };

            // Act
            await _service.UpdateSubjectAsync(subject.Id, updateDto);

            // Assert
            var updated = await _context.Subjects.FindAsync(subject.Id);
            Assert.NotNull(updated);
            Assert.Equal("Physics New", updated.Name);
            Assert.Equal("PHY101", updated.Code);
        }

        [Fact]
        public async Task UpdateSubjectAsync_ShouldDoNothing_WhenNotExists()
        {
            // Act & Assert (Should not throw)
            await _service.UpdateSubjectAsync(Guid.NewGuid(), new SubjectUpdateDto { Name = "None" });
        }

        [Fact]
        public async Task DeleteSubjectAsync_ShouldRemoveSubject_WhenExists()
        {
            // Arrange
            var subject = new Subject { Id = Guid.NewGuid(), Name = "Biology", Code = "BIO101" };
            _context.Subjects.Add(subject);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteSubjectAsync(subject.Id);

            // Assert
            var deleted = await _context.Subjects.FindAsync(subject.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteSubjectAsync_ShouldDoNothing_WhenNotExists()
        {
            // Act & Assert (Should not throw)
            await _service.DeleteSubjectAsync(Guid.NewGuid());
        }

        [Fact]
        public async Task GetSubjectsAsync_ShouldFilterByClassId_AndPopulateLinkedClasses()
        {
            // Arrange
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub1 = new Subject { Id = Guid.NewGuid(), Name = "Algebra", Code = "ALG101" };
            var sub2 = new Subject { Id = Guid.NewGuid(), Name = "Geometry", Code = "GEO101" };
            var cs = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls.Id, SubjectId = sub1.Id, Class = cls, Subject = sub1 };

            _context.Classes.Add(cls);
            _context.Subjects.AddRange(sub1, sub2);
            _context.ClassSubjects.Add(cs);
            await _context.SaveChangesAsync();

            var filter = new SubjectFilterDto
            {
                ClassId = cls.Id,
                PageNumber = 1,
                PageSize = 10,
                SortBy = "name",
                SortOrder = SortOrder.Asc
            };

            // Act
            var result = await _service.GetSubjectsAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Single(items);
            Assert.Equal("Algebra", items[0].Name);
            var linkedClasses = items[0].LinkedClasses.ToList();
            Assert.Single(linkedClasses);
            Assert.Equal("Class 10", linkedClasses[0].Name);
        }

        [Fact]
        public async Task GetSubjectsAsync_ShouldSortByCode_Descending()
        {
            // Arrange
            _context.Subjects.AddRange(
                new Subject { Id = Guid.NewGuid(), Name = "Subject A", Code = "A100" },
                new Subject { Id = Guid.NewGuid(), Name = "Subject B", Code = "B200" }
            );
            await _context.SaveChangesAsync();

            var filter = new SubjectFilterDto
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "code",
                SortOrder = SortOrder.Desc
            };

            // Act
            var result = await _service.GetSubjectsAsync(filter);

            // Assert
            Assert.Equal(2, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Equal("B200", items[0].Code);
            Assert.Equal("A100", items[1].Code);
        }
    }
}
