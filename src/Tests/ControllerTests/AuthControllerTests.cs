using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.ControllerTests
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly DefaultHttpContext _httpContext;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            (_mockHttpContextAccessor, _httpContext) = MockHelper.CreateMockHttpContext();

            var authHandler = new AuthHandler(_mockUserService.Object, _mockHttpContextAccessor.Object, Mock.Of<ILogger<AuthHandler>>());
            var userAuthHandler = new UserAuthHandler(_mockUserService.Object, Mock.Of<ILogger<UserAuthHandler>>());

            _controller = new AuthController(authHandler, userAuthHandler, Mock.Of<ILogger<AuthController>>());
        }

        [Fact]
        public async Task CreateUser_ShouldReturnCreatedStatus_WithUserData()
        {
            // Arrange
            var dto = new UserCreateDto
            {
                FullName = "John Doe",
                Email = "john@example.com",
                Password = "Password123!",
                Role = UserRole.Student
            };
            var createdUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role
            };
            _mockUserService.Setup(s => s.CreateUserAsync(dto)).ReturnsAsync(createdUser);

            // Act
            var result = await _controller.CreateUser(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithToken_WhenCredentialsValid()
        {
            // Arrange
            var dto = new UserLoginDto { Email = "john@example.com", Password = "Password123!" };
            var user = new User { Id = Guid.NewGuid(), Email = dto.Email, FullName = "John" };
            _mockUserService.Setup(s => s.AuthenticateUserAsync(dto.Email, dto.Password)).ReturnsAsync(user);
            _mockUserService.Setup(s => s.GenerateJwtToken(user)).ReturnsAsync("jwt_token_xyz");
            _mockUserService.Setup(s => s.GenerateRefreshToken(user)).ReturnsAsync("refresh_token_abc");

            // Act
            var result = await _controller.Login(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task RefreshToken_ShouldReturnOk_WithNewToken_WhenCookieValid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "john@example.com" };
            _httpContext.Request.Headers["Cookie"] = "refreshToken=valid_refresh_token";

            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("valid_refresh_token")).ReturnsAsync(user);
            _mockUserService.Setup(s => s.GenerateJwtToken(user)).ReturnsAsync("new_jwt_token");
            _mockUserService.Setup(s => s.GenerateRefreshToken(user)).ReturnsAsync("new_refresh_token");

            // Act
            var result = await _controller.RefreshToken();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Logout_ShouldReturnOk_WhenTokensValid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "john@example.com" };
            _httpContext.Request.Headers["Cookie"] = "refreshToken=valid_refresh_token";
            _httpContext.Request.Headers["Authorization"] = "Bearer valid_jwt_token";

            _mockUserService.Setup(s => s.GetUserByRefreshTokenAsync("valid_refresh_token")).ReturnsAsync(user);
            _mockUserService.Setup(s => s.InvalidateRefreshTokenAndJwtToken("valid_jwt_token", "valid_refresh_token"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task UpdateUser_ShouldReturnOk_WhenUserExistsAndIdMatches()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UserUpdateDto { Id = id, FullName = "John Updated" };
            var existing = new User { Id = id, FullName = "John" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(existing);
            _mockUserService.Setup(s => s.UpdateUserAsync(id, dto)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateUser(dto, id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task DeleteUser_ShouldReturnOk_WhenUserExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new User { Id = id, FullName = "John" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(existing);
            _mockUserService.Setup(s => s.DeleteUserAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteUser(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}
