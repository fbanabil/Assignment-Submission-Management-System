using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.HandlerTests.Auth
{
    public class UserAuthHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly UserAuthHandler _handler;

        public UserAuthHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _handler = new UserAuthHandler(_mockUserService.Object);
        }

        [Fact]
        public async Task HandleCreateUserAsync_ShouldReturnCreatedResult()
        {
            // Arrange
            var dto = new UserCreateDto
            {
                FullName = "New User",
                Email = "new@test.com",
                Password = "Password123",
                Role = UserRole.Student
            };
            var created = new User
            {
                Id = Guid.NewGuid(),
                FullName = dto.FullName,
                Email = dto.Email,
                Role = dto.Role
            };
            _mockUserService.Setup(s => s.CreateUserAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _handler.HandleCreateUserAsync(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task HandleUpdateUserAsync_ShouldReturnForbidden_WhenIdMismatches()
        {
            // Arrange
            var routeId = Guid.NewGuid();
            var dto = new UserUpdateDto { Id = Guid.NewGuid(), FullName = "Updated" };

            // Act
            var result = await _handler.HandleUpdateUserAsync(dto, routeId);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status403Forbidden, objectResult.StatusCode);
        }

        [Fact]
        public async Task HandleUpdateUserAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UserUpdateDto { Id = id, FullName = "Updated" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.HandleUpdateUserAsync(dto, id);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
        }

        [Fact]
        public async Task HandleUpdateUserAsync_ShouldReturnOk_WhenUpdateSucceeds()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UserUpdateDto { Id = id, FullName = "Updated" };
            var existing = new User { Id = id, FullName = "Original" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(existing);
            _mockUserService.Setup(s => s.UpdateUserAsync(id, dto)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleUpdateUserAsync(dto, id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockUserService.Verify(s => s.UpdateUserAsync(id, dto), Times.Once);
        }

        [Fact]
        public async Task HandleDeleteUserAsync_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync((User?)null);

            // Act
            var result = await _handler.HandleDeleteUserAsync(id);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
        }

        [Fact]
        public async Task HandleDeleteUserAsync_ShouldReturnOk_WhenUserDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new User { Id = id, FullName = "To Delete" };
            _mockUserService.Setup(s => s.GetUserByIdAsync(id)).ReturnsAsync(existing);
            _mockUserService.Setup(s => s.DeleteUserAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleDeleteUserAsync(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockUserService.Verify(s => s.DeleteUserAsync(id), Times.Once);
        }
    }
}
