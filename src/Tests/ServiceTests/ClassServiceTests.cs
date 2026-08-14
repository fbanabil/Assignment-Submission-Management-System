using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.ClassDTOs;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class ClassServiceTests
    {
        private readonly AppDbContext _context;
        private readonly ClassService _service;

        public ClassServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _service = new ClassService(_context, Mock.Of<ILogger<ClassService>>());
        }

        [Fact]
        public async Task GetAllClassesAsync_ShouldReturnAllClasses()
        {
            // Arrange
            _context.Classes.AddRange(
                new Class { Id = Guid.NewGuid(), Name = "Class 1", Section = "A", AcademicYear = "2026", CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 2", Section = "B", AcademicYear = "2026", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllClassesAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetClassByIdAsync_ShouldReturnClass_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var cls = new Class { Id = id, Name = "Class 10", Section = "A", AcademicYear = "2026", CreatedAt = DateTime.UtcNow };
            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetClassByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Class 10", result.Name);
        }

        [Fact]
        public async Task GetClassByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            // Act
            var result = await _service.GetClassByIdAsync(Guid.NewGuid());

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CreateClassAsync_ShouldAddAndReturnClass()
        {
            // Arrange
            var dto = new ClassCreateDto
            {
                Name = "Grade 9",
                Section = "B",
                AcademicYear = "2026-2027"
            };

            // Act
            var created = await _service.CreateClassAsync(dto);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(dto.Name, created.Name);
            Assert.Equal(dto.Section, created.Section);
            Assert.Equal(dto.AcademicYear, created.AcademicYear);
            Assert.NotEqual(Guid.Empty, created.Id);

            var dbClass = await _context.Classes.FindAsync(created.Id);
            Assert.NotNull(dbClass);
        }

        [Fact]
        public async Task UpdateClassAsync_ShouldUpdateProperties_WhenClassExists()
        {
            // Arrange
            var cls = new Class { Id = Guid.NewGuid(), Name = "Old Name", Section = "A", AcademicYear = "2025", CreatedAt = DateTime.UtcNow };
            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();

            var updateDto = new ClassUpdateDto
            {
                Name = "New Name",
                Section = "C",
                AcademicYear = "2026"
            };

            // Act
            await _service.UpdateClassAsync(cls.Id, updateDto);

            // Assert
            var updated = await _context.Classes.FindAsync(cls.Id);
            Assert.NotNull(updated);
            Assert.Equal("New Name", updated.Name);
            Assert.Equal("C", updated.Section);
            Assert.Equal("2026", updated.AcademicYear);
        }

        [Fact]
        public async Task UpdateClassAsync_ShouldDoNothing_WhenClassDoesNotExist()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();
            var updateDto = new ClassUpdateDto { Name = "New Name" };

            // Act & Assert (Should not throw)
            await _service.UpdateClassAsync(nonExistentId, updateDto);
        }

        [Fact]
        public async Task DeleteClassAsync_ShouldRemoveClass_WhenExists()
        {
            // Arrange
            var cls = new Class { Id = Guid.NewGuid(), Name = "To Delete", Section = "A", AcademicYear = "2026", CreatedAt = DateTime.UtcNow };
            _context.Classes.Add(cls);
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteClassAsync(cls.Id);

            // Assert
            var deleted = await _context.Classes.FindAsync(cls.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteClassAsync_ShouldDoNothing_WhenNotExists()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act & Assert (Should not throw)
            await _service.DeleteClassAsync(nonExistentId);
        }

        [Fact]
        public async Task GetClassesAsync_ShouldPaginateAndSort_Correctly()
        {
            // Arrange
            _context.Classes.AddRange(
                new Class { Id = Guid.NewGuid(), Name = "Alpha Class", Section = "A", AcademicYear = "2026", CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Class { Id = Guid.NewGuid(), Name = "Beta Class", Section = "B", AcademicYear = "2026", CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new Class { Id = Guid.NewGuid(), Name = "Gamma Class", Section = "A", AcademicYear = "2025", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var filter = new ClassFilterDto
            {
                PageNumber = 1,
                PageSize = 2,
                SortBy = "name",
                SortOrder = SortOrder.Asc
            };

            // Act
            var result = await _service.GetClassesAsync(filter);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Equal(2, items.Count);
            Assert.Equal("Alpha Class", items[0].Name);
            Assert.Equal("Beta Class", items[1].Name);
        }

        [Fact]
        public async Task GetClassesAsync_ShouldSortDescending_BySection()
        {
            // Arrange
            _context.Classes.AddRange(
                new Class { Id = Guid.NewGuid(), Name = "Class 1", Section = "A", AcademicYear = "2026", CreatedAt = DateTime.UtcNow },
                new Class { Id = Guid.NewGuid(), Name = "Class 2", Section = "Z", AcademicYear = "2026", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            var filter = new ClassFilterDto
            {
                PageNumber = 1,
                PageSize = 10,
                SortBy = "section",
                SortOrder = SortOrder.Desc
            };

            // Act
            var result = await _service.GetClassesAsync(filter);

            // Assert
            Assert.Equal(2, result.TotalCount);
            var items = result.Items.ToList();
            Assert.Equal("Z", items[0].Section);
            Assert.Equal("A", items[1].Section);
        }
    }
}
