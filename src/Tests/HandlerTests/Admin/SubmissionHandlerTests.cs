using Backend.DTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class SubmissionHandlerTests
    {
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly SubmissionHandler _handler;

        public SubmissionHandlerTests()
        {
            _mockSubmissionService = new Mock<ISubmissionService>();
            _handler = new SubmissionHandler(_mockSubmissionService.Object, Mock.Of<ILogger<SubmissionHandler>>());
        }

        [Fact]
        public async Task HandleGetSubmissionsAsync_ShouldReturnBadRequest_WhenFilterIsNull()
        {
            // Act
            var result = await _handler.HandleGetSubmissionsAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Filter parameters are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleGetSubmissionsAsync_ShouldReturnOk_WithPagedSubmissions()
        {
            // Arrange
            var filter = new SubmissionFilterDto();
            var paged = new PagedResultDto<SubmissionResponseDto> { Items = new List<SubmissionResponseDto>(), TotalCount = 0 };
            _mockSubmissionService.Setup(s => s.GetSubmissionsAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetSubmissionsAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
