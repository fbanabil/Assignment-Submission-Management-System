using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Teacher;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Teacher
{
    public class TeacherAssignmentHandlerTests
    {
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly TeacherAssignmentHandler _handler;

        public TeacherAssignmentHandlerTests()
        {
            _mockAssignmentService = new Mock<IAssignmentService>();
            _handler = new TeacherAssignmentHandler(_mockAssignmentService.Object, Mock.Of<ILogger<TeacherAssignmentHandler>>());
        }

        [Fact]
        public async Task HandleGetAssignmentsAsync_ShouldThrowBadRequest_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleGetAssignmentsAsync(null!));
        }

        [Fact]
        public async Task HandleGetAssignmentsAsync_ShouldReturnOk_WithPagedResult()
        {
            // Arrange
            var dto = new AssignmentFilterDto();
            var paged = new PagedResultDto<AssignmentResponseDto> { Items = new List<AssignmentResponseDto>(), TotalCount = 0 };
            _mockAssignmentService.Setup(s => s.GetAssignmentsForTeacher(dto)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetAssignmentsAsync(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleCreateAssignmentAsync_ShouldThrowBadRequest_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleCreateAssignmentAsync(null!));
        }

        [Fact]
        public async Task HandleCreateAssignmentAsync_ShouldReturnCreatedStatus_WhenValid()
        {
            // Arrange
            var dto = new AssignmentCreateDto { Title = "Assignment 1" };
            var response = new AssignmentResponseDto { Id = Guid.NewGuid(), Title = dto.Title };
            _mockAssignmentService.Setup(s => s.CreateAssignmentAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _handler.HandleCreateAssignmentAsync(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal(response, objectResult.Value);
        }

        [Fact]
        public async Task HandleUpdateAssignmentAsync_ShouldThrowBadRequest_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleUpdateAssignmentAsync(Guid.NewGuid(), null!));
        }

        [Fact]
        public async Task HandleUpdateAssignmentAsync_ShouldReturnNoContent_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new AssignmentUpdateDto { Title = "Updated" };
            _mockAssignmentService.Setup(s => s.UpdateAssignmentAsync(id, dto)).ReturnsAsync(new AssignmentResponseDto());

            // Act
            var result = await _handler.HandleUpdateAssignmentAsync(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockAssignmentService.Verify(s => s.UpdateAssignmentAsync(id, dto), Times.Once);
        }
    }
}
