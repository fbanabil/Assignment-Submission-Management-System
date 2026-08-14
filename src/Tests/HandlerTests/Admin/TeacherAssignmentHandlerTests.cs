using AssignmentSystem.Api.Models.Entities;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class TeacherAssignmentHandlerTests
    {
        private readonly Mock<ITeacherAssignmentService> _mockTeacherAssignmentService;
        private readonly TeacherAssignmentHandler _handler;

        public TeacherAssignmentHandlerTests()
        {
            _mockTeacherAssignmentService = new Mock<ITeacherAssignmentService>();
            _handler = new TeacherAssignmentHandler(_mockTeacherAssignmentService.Object);
        }

        [Fact]
        public async Task HandleGetTeacherAssignmentsAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleGetTeacherAssignmentsAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Filter parameters are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleGetTeacherAssignmentsAsync_ShouldReturnOk_WithPagedData()
        {
            // Arrange
            var dto = new TeacherAssignmentFilterDto();
            var paged = new PagedResultDto<TeacherAssignmentResponseDto> { Items = new List<TeacherAssignmentResponseDto>(), TotalCount = 0 };
            _mockTeacherAssignmentService.Setup(s => s.GetTeacherAssignmentsAsync(dto)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetTeacherAssignmentsAsync(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleCreateTeacherAssignmentAsync_ShouldReturnBadRequest_WhenDtoIsNull()
        {
            // Act
            var result = await _handler.HandleCreateTeacherAssignmentAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("TeacherAssignment data is required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleCreateTeacherAssignmentAsync_ShouldReturnCreatedAtAction_WhenValid()
        {
            // Arrange
            var dto = new TeacherAssignmentCreateDto { TeacherId = Guid.NewGuid(), ClassSubjectId = Guid.NewGuid() };
            var created = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = dto.TeacherId, ClassSubjectId = dto.ClassSubjectId };
            _mockTeacherAssignmentService.Setup(s => s.CreateTeacherAssignmentAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _handler.HandleCreateTeacherAssignmentAsync(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AdminController.TeacherAssignments), createdResult.ActionName);
        }

        [Fact]
        public async Task HandleDeleteTeacherAssignmentAsync_ShouldReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockTeacherAssignmentService.Setup(s => s.DeleteTeacherAssignmentAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleDeleteTeacherAssignmentAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockTeacherAssignmentService.Verify(s => s.DeleteTeacherAssignmentAsync(id), Times.Once);
        }
    }
}
