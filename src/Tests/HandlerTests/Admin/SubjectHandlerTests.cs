using AssignmentSystem.Api.Models.Entities;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class SubjectHandlerTests
    {
        private readonly Mock<ISubjectService> _mockSubjectService;
        private readonly SubjectHandler _handler;

        public SubjectHandlerTests()
        {
            _mockSubjectService = new Mock<ISubjectService>();
            _handler = new SubjectHandler(_mockSubjectService.Object, Mock.Of<ILogger<SubjectHandler>>());
        }

        [Fact]
        public async Task HandleGetSubjectsAsync_ShouldReturnBadRequest_WhenFilterIsNull()
        {
            // Act
            var result = await _handler.HandleGetSubjectsAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Filter parameters are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleGetSubjectsAsync_ShouldReturnOk_WithPagedSubjects()
        {
            // Arrange
            var filter = new SubjectFilterDto();
            var paged = new PagedResultDto<SubjectResponseDto> { Items = new List<SubjectResponseDto>(), TotalCount = 0 };
            _mockSubjectService.Setup(s => s.GetSubjectsAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetSubjectsAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleCreateSubjectAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleCreateSubjectAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Subject data is required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleCreateSubjectAsync_ShouldReturnCreatedAtAction_WhenValid()
        {
            // Arrange
            var dto = new SubjectCreateDto { Name = "Science", Code = "SCI101" };
            var created = new Subject { Id = Guid.NewGuid(), Name = dto.Name, Code = dto.Code };
            _mockSubjectService.Setup(s => s.CreateSubjectAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _handler.HandleCreateSubjectAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AdminController.Subjects), createdResult.ActionName);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleUpdateAsync(Guid.NewGuid(), null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Subject data is required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleUpdateAsync_ShouldReturnNoContent_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new SubjectUpdateDto { Name = "Advanced Science" };
            _mockSubjectService.Setup(s => s.UpdateSubjectAsync(id, dto)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleUpdateAsync(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubjectService.Verify(s => s.UpdateSubjectAsync(id, dto), Times.Once);
        }

        [Fact]
        public async Task HandleDeleteSubjectAsync_ShouldReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockSubjectService.Setup(s => s.DeleteSubjectAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleDeleteSubjectAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubjectService.Verify(s => s.DeleteSubjectAsync(id), Times.Once);
        }
    }
}
