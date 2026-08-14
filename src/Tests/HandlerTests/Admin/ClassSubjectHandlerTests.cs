using AssignmentSystem.Api.Models.Entities;
using Backend.Controllers;
using Backend.DTOs.ClassSubjectDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class ClassSubjectHandlerTests
    {
        private readonly Mock<IClassSubjectService> _mockClassSubjectService;
        private readonly ClassSubjectHandler _handler;

        public ClassSubjectHandlerTests()
        {
            _mockClassSubjectService = new Mock<IClassSubjectService>();
            _handler = new ClassSubjectHandler(_mockClassSubjectService.Object, Mock.Of<ILogger<ClassSubjectHandler>>());
        }

        [Fact]
        public async Task HandleCreateClassSubjectAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleCreateClassSubjectAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ClassSubject data is required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleCreateClassSubjectAsync_ShouldReturnCreatedAtAction_WhenValid()
        {
            // Arrange
            var dto = new ClassSubjectCreateDto { ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid() };
            var created = new ClassSubject { Id = Guid.NewGuid(), ClassId = dto.ClassId, SubjectId = dto.SubjectId };
            _mockClassSubjectService.Setup(s => s.CreateClassSubjectAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _handler.HandleCreateClassSubjectAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AdminController.ClassSubjects), createdResult.ActionName);
        }

        [Fact]
        public async Task HandleDeleteClassSubjectAsync_ShouldReturnBadRequest_WhenGuidsEmpty()
        {
            // Act
            var result = await _handler.HandleDeleteClassSubjectAsync(Guid.Empty, Guid.NewGuid());

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("ClassId and SubjectId are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleDeleteClassSubjectAsync_ShouldReturnNotFound_WhenAssociationDoesNotExist()
        {
            // Arrange
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            _mockClassSubjectService.Setup(s => s.GetAllClassSubjectsAsync())
                .ReturnsAsync(new List<ClassSubject>());

            // Act
            var result = await _handler.HandleDeleteClassSubjectAsync(classId, subjectId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("ClassSubject association not found.", notFound.Value);
        }

        [Fact]
        public async Task HandleDeleteClassSubjectAsync_ShouldReturnNoContent_WhenFoundAndDeleted()
        {
            // Arrange
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var csId = Guid.NewGuid();
            _mockClassSubjectService.Setup(s => s.GetAllClassSubjectsAsync())
                .ReturnsAsync(new List<ClassSubject> { new ClassSubject { Id = csId, ClassId = classId, SubjectId = subjectId } });
            _mockClassSubjectService.Setup(s => s.DeleteClassSubjectAsync(csId)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleDeleteClassSubjectAsync(classId, subjectId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockClassSubjectService.Verify(s => s.DeleteClassSubjectAsync(csId), Times.Once);
        }
    }
}
