using Backend.DTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class UserHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserHandler _handler;

        public UserHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _handler = new UserHandler(_mockUserService.Object, Mock.Of<ILogger<UserHandler>>());
        }

        [Fact]
        public async Task HandleGetUsersAsync_ShouldReturnBadRequest_WhenFilterIsNull()
        {
            // Act
            var result = await _handler.HandleGetUsersAsync(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Filter parameters are required.", badRequest.Value);
        }

        [Fact]
        public async Task HandleGetUsersAsync_ShouldReturnOk_WithPagedUsers()
        {
            // Arrange
            var filter = new UserFilterDto();
            var paged = new PagedResultDto<UserResponseDto> { Items = new List<UserResponseDto>(), TotalCount = 0 };
            _mockUserService.Setup(s => s.GetUsersAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetUsersAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
