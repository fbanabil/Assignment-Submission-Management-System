using AssignmentSystem.Api.Models.Entities;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class ClassHandlerTests
    {
        private readonly Mock<IClassService> _mockClassService;
        private readonly ClassHandler _handler;

        public ClassHandlerTests()
        {
            _mockClassService = new Mock<IClassService>();
            _handler = new ClassHandler(_mockClassService.Object, Mock.Of<ILogger<ClassHandler>>());
        }

        [Fact]
        public async Task HandleGetClassesAsync_ShouldReturnBadRequest_WhenFilterIsNull()
        {
            // Act
            var result = await _handler.HandleGetClassesAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Filter parameters are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleGetClassesAsync_ShouldReturnOk_WithPagedClasses()
        {
            // Arrange
            var filter = new ClassFilterDto();
            var paged = new PagedResultDto<ClassResponseDto> { Items = new List<ClassResponseDto>(), TotalCount = 0 };
            _mockClassService.Setup(s => s.GetClassesAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetClassesAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleCreateClassAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleCreateClassAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Class data is required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleCreateClassAsync_ShouldReturnCreatedAtAction_WhenValid()
        {
            // Arrange
            var dto = new ClassCreateDto { Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var created = new Class { Id = Guid.NewGuid(), Name = dto.Name };
            _mockClassService.Setup(s => s.CreateClassAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _handler.HandleCreateClassAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AdminController.Classes), createdResult.ActionName);
        }

        [Fact]
        public async Task HandleUpdateClassAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleUpdateClassAsync(Guid.NewGuid(), null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Class data is required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleUpdateClassAsync_ShouldReturnNoContent_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new ClassUpdateDto { Name = "Updated" };
            _mockClassService.Setup(s => s.UpdateClassAsync(id, dto)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleUpdateClassAsync(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockClassService.Verify(s => s.UpdateClassAsync(id, dto), Times.Once);
        }

        [Fact]
        public async Task HandleDeleteClassAsync_ShouldReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockClassService.Setup(s => s.DeleteClassAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleDeleteClassAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockClassService.Verify(s => s.DeleteClassAsync(id), Times.Once);
        }
    }
}
