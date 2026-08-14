using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class AssignmentHandlerTests
    {
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly AssignmentHandler _handler;

        public AssignmentHandlerTests()
        {
            _mockAssignmentService = new Mock<IAssignmentService>();
            _handler = new AssignmentHandler(_mockAssignmentService.Object);
        }

        [Fact]
        public async Task HandleGetAssignmentsAsync_ShouldReturnBadRequest_WhenFilterIsNull()
        {
            // Act
            var result = await _handler.HandleGetAssignmentsAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Filter parameters are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleGetAssignmentsAsync_ShouldReturnOk_WithPagedResult()
        {
            // Arrange
            var filter = new AssignmentFilterDto();
            var paged = new PagedResultDto<AssignmentResponseDto> { Items = new List<AssignmentResponseDto>(), TotalCount = 0 };
            _mockAssignmentService.Setup(s => s.GetAssignmentsAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetAssignmentsAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
