using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Auth;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Auth
{
    public class AuthHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly DefaultHttpContext _httpContext;
        private readonly AuthHandler _handler;

        public AuthHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            (_mockHttpContextAccessor, _httpContext) = MockHelper.CreateMockHttpContext();
            _handler = new AuthHandler(_mockUserService.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task HandleLoginAsync_ShouldThrowBadRequest_WhenCredentialsInvalid()
        {
            // Arrange
            var dto = new UserLoginDto { Email = "wrong@test.com", Password = "bad" };
            _mockUserService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ReturnsAsync((User?)null);

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleLoginAsync(dto));
        }

        [Fact]
        public async Task HandleLoginAsync_ShouldReturnOk_AndSetCookie_WhenCredentialsValid()
        {
            // Arrange
            var dto = new UserLoginDto { Email = "user@test.com", Password = "Password123" };
            var user = new User { Id = Guid.NewGuid(), Email = dto.Email, FullName = "Test User" };
            _mockUserService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.GenerateJwtToken(user)).ReturnsAsync("valid_jwt");
            _mockUserService.Setup(s => s.GenerateRefreshToken(user)).ReturnsAsync("valid_refresh");

            // Act
            var result = await _handler.HandleLoginAsync(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_ShouldReturnUnauthorized_WhenCookieMissing()
        {
            // Act
            var result = await _handler.HandleRefreshTokenAsync();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_ShouldReturnUnauthorized_WhenTokenInvalid()
        {
            // Arrange
            _httpContext.Request.Headers["Cookie"] = "refreshToken=invalid_token";
            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("invalid_token")).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.HandleRefreshTokenAsync();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
        }

        [Fact]
        public async Task HandleRefreshTokenAsync_ShouldReturnOk_WithNewTokens_WhenValid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "user@test.com" };
            _httpContext.Request.Headers["Cookie"] = "refreshToken=valid_token";
            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("valid_token")).ReturnsAsync(user);
            _mockUserService.Setup(s => s.GenerateJwtToken(user)).ReturnsAsync("new_jwt");
            _mockUserService.Setup(s => s.GenerateRefreshToken(user)).ReturnsAsync("new_refresh");

            // Act
            var result = await _handler.HandleRefreshTokenAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task HandleLogoutAsync_ShouldReturnUnauthorized_WhenCookieMissing()
        {
            // Act
            var result = await _handler.HandleLogoutAsync();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
        }

        [Fact]
        public async Task HandleLogoutAsync_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            // Arrange
            _httpContext.Request.Headers["Cookie"] = "refreshToken=unknown_token";
            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("unknown_token")).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.HandleLogoutAsync();

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.NotNull(unauthorized.Value);
        }

        [Fact]
        public async Task HandleLogoutAsync_ShouldReturnBadRequest_WhenAuthHeaderMissing()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "user@test.com" };
            _httpContext.Request.Headers["Cookie"] = "refreshToken=valid_token";
            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("valid_token")).ReturnsAsync(user);

            // Act
            var result = await _handler.HandleLogoutAsync();

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task HandleLogoutAsync_ShouldReturnOk_WhenTokensValid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "user@test.com" };
            _httpContext.Request.Headers["Cookie"] = "refreshToken=valid_token";
            _httpContext.Request.Headers["Authorization"] = "Bearer jwt_123";
            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("valid_token")).ReturnsAsync(user);
            _mockUserService.Setup(s => s.InvalidateRefreshTokenAndJwtToken("jwt_123", "valid_token"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleLogoutAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockUserService.Verify(s => s.InvalidateRefreshTokenAndJwtToken("jwt_123", "valid_token"), Times.Once);
        }
    }
}
