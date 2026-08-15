using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Services;
using Backend.DTOs.UserDTOs;
using Backend.Helpers;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Tests.Helpers;

namespace Tests.ServiceTests
{
    public class UserServiceTests
    {
        private readonly AppDbContext _context;
        private readonly Mock<IPasswordHelper> _mockPasswordHelper;
        private readonly Mock<ILogger<UserService>> _mockLogger;
        private readonly Mock<IAuthenticationHelper> _mockAuthHelper;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ITokenBlacklistRepository> _mockBlacklistRepo;
        private readonly DefaultHttpContext _httpContext;
        private readonly UserService _service;

        public UserServiceTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
            _mockPasswordHelper = new Mock<IPasswordHelper>();
            _mockLogger = new Mock<ILogger<UserService>>();
            _mockAuthHelper = new Mock<IAuthenticationHelper>();
            _mockBlacklistRepo = new Mock<ITokenBlacklistRepository>();

            (_mockHttpContextAccessor, _httpContext) = MockHelper.CreateMockHttpContext();

            _service = new UserService(
                _context,
                _mockPasswordHelper.Object,
                _mockLogger.Object,
                _mockAuthHelper.Object,
                _mockHttpContextAccessor.Object,
                _mockBlacklistRepo.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_ShouldReturnAllUsers()
        {
            // Arrange
            _context.Users.AddRange(
                new User { Id = Guid.NewGuid(), FullName = "User 1", Email = "u1@test.com", PasswordHash = "h", Role = UserRole.Student },
                new User { Id = Guid.NewGuid(), FullName = "User 2", Email = "u2@test.com", PasswordHash = "h", Role = UserRole.Teacher }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user = new User { Id = id, FullName = "John", Email = "john@test.com", PasswordHash = "h", Role = UserRole.Student };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserByIdAsync(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("John", result.FullName);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrowBadRequest_WhenEmailAlreadyExists()
        {
            // Arrange
            _context.Users.Add(new User { Id = Guid.NewGuid(), FullName = "Existing", Email = "duplicate@test.com", PasswordHash = "h", Role = UserRole.Student });
            await _context.SaveChangesAsync();

            var dto = new UserCreateDto { FullName = "New", Email = "duplicate@test.com", Password = "Pass", Role = UserRole.Student };

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateUserAsync(dto));
        }

        [Fact]
        public async Task CreateUserAsync_ShouldHashPassword_AndSaveUser()
        {
            // Arrange
            var dto = new UserCreateDto
            {
                FullName = "Alice",
                Email = "alice@test.com",
                Password = "PlainPassword123",
                Role = UserRole.Student,
                RollNo = "ST-001"
            };
            _mockPasswordHelper.Setup(p => p.HashPassword("PlainPassword123")).ReturnsAsync("hashed_pass_123");

            // Act
            var created = await _service.CreateUserAsync(dto);

            // Assert
            Assert.NotNull(created);
            Assert.Equal("alice@test.com", created.Email);
            Assert.Equal("hashed_pass_123", created.PasswordHash);
            Assert.Equal("ST-001", created.RollNo);
            Assert.True(created.IsActive);
        }

        [Fact]
        public async Task UpdateUserAsync_ShouldUpdateProperties_AndClearRollNoIfNotStudent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var user = new User { Id = id, FullName = "Bob", Email = "bob@test.com", PasswordHash = "h", Role = UserRole.Student, RollNo = "R-10" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var updateDto = new UserUpdateDto
            {
                FullName = "Bob Teacher",
                Role = UserRole.Teacher
            };

            // Act
            await _service.UpdateUserAsync(id, updateDto);

            // Assert
            var updated = await _context.Users.FindAsync(id);
            Assert.NotNull(updated);
            Assert.Equal("Bob Teacher", updated.FullName);
            Assert.Equal(UserRole.Teacher, updated.Role);
            Assert.Null(updated.RollNo);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldRemoveUser()
        {
            // Arrange
            var id = Guid.NewGuid();
            _context.Users.Add(new User { Id = id, FullName = "Delete Me", Email = "del@test.com", PasswordHash = "h", Role = UserRole.Student });
            await _context.SaveChangesAsync();

            // Act
            await _service.DeleteUserAsync(id);

            // Assert
            var user = await _context.Users.FindAsync(id);
            Assert.Null(user);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnUser_WhenCredentialsValid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "auth@test.com", PasswordHash = "hashed_pass", FullName = "Auth User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockPasswordHelper.Setup(p => p.VerifyPassword("correct_pass", "hashed_pass")).ReturnsAsync(true);

            // Act
            var result = await _service.AuthenticateUserAsync("auth@test.com", "correct_pass");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("auth@test.com", result.Email);
        }

        [Fact]
        public async Task AuthenticateUserAsync_ShouldReturnNull_WhenPasswordInvalid()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "auth@test.com", PasswordHash = "hashed_pass", FullName = "Auth User" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _mockPasswordHelper.Setup(p => p.VerifyPassword("wrong_pass", "hashed_pass")).ReturnsAsync(false);

            // Act
            var result = await _service.AuthenticateUserAsync("auth@test.com", "wrong_pass");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GenerateJwtToken_ShouldCallAuthHelper()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), FullName = "User A", Email = "a@test.com", Role = UserRole.Admin };
            _mockAuthHelper.Setup(a => a.CreateJwtToken(It.IsAny<UserPayload>())).ReturnsAsync("mock_jwt");

            // Act
            var token = await _service.GenerateJwtToken(user);

            // Assert
            Assert.Equal("mock_jwt", token);
        }

        [Fact]
        public async Task GenerateRefreshToken_ShouldSaveHashedTokenInDb()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "a@test.com", FullName = "User A" };
            _mockAuthHelper.Setup(a => a.CreateRefreshTokenAsync()).ReturnsAsync("raw_refresh_token");
            _mockAuthHelper.Setup(a => a.HashTokenAsync("raw_refresh_token")).ReturnsAsync("hashed_refresh_token");

            // Act
            var token = await _service.GenerateRefreshToken(user);

            // Assert
            Assert.Equal("raw_refresh_token", token);
            var savedToken = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == "hashed_refresh_token");
            Assert.NotNull(savedToken);
            Assert.Equal(user.Id, savedToken.UserId);
            Assert.False(savedToken.IsUsed);
        }

        [Fact]
        public async Task GetUserByRefreshTokenAsync_ShouldReturnUser_WhenTokenValidAndMatchesClaims()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "match@test.com", FullName = "Match User" };
            _context.Users.Add(user);
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "hashed_valid_token",
                UserId = user.Id,
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();

            // Set up claims to match user.Id
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            }, "Test"));

            _mockAuthHelper.Setup(a => a.HashTokenAsync("raw_token")).ReturnsAsync("hashed_valid_token");

            // Act
            var result = await _service.GetUserByRefreshTokenAsync("raw_token");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Id, result.Id);
            var dbToken = await _context.RefreshTokens.FirstAsync();
            Assert.True(dbToken.IsUsed);
        }



        [Fact]
        public async Task InvalidateRefreshTokenAndJwtToken_ShouldMarkTokenUsed_AndBlacklistJwt()
        {
            // Arrange
            _context.RefreshTokens.Add(new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "hashed_token_to_invalidate",
                UserId = Guid.NewGuid(),
                IsUsed = false,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            });
            await _context.SaveChangesAsync();

            _mockAuthHelper.Setup(a => a.HashTokenAsync("cookie_token")).ReturnsAsync("hashed_token_to_invalidate");
            _mockBlacklistRepo.Setup(b => b.AddToBlacklistAsync("jwt_token_123", It.IsAny<TimeSpan>()))
                .Returns(Task.CompletedTask);

            // Act
            await _service.InvalidateRefreshTokenAndJwtToken("jwt_token_123", "cookie_token");

            // Assert
            var token = await _context.RefreshTokens.FirstAsync();
            Assert.True(token.IsUsed);
            _mockBlacklistRepo.Verify(b => b.AddToBlacklistAsync("jwt_token_123", TimeSpan.FromDays(1)), Times.Once);
        }

        [Fact]
        public async Task InvalidateRefreshTokenAndJwtToken_ShouldThrowBadRequest_WhenTokenNotFound()
        {
            // Arrange
            _mockAuthHelper.Setup(a => a.HashTokenAsync("missing_token")).ReturnsAsync("hashed_missing_token");

            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _service.InvalidateRefreshTokenAndJwtToken("jwt_token", "missing_token"));
        }

        [Fact]
        public async Task GetUserSummaryAsync_ShouldCalculateCorrectMetrics()
        {
            // Arrange
            _context.Users.AddRange(
                new User { Id = Guid.NewGuid(), FullName = "U1", Email = "u1@test.com", PasswordHash = "h", Role = UserRole.Student, IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { Id = Guid.NewGuid(), FullName = "U2", Email = "u2@test.com", PasswordHash = "h", Role = UserRole.Teacher, IsActive = true, CreatedAt = DateTime.UtcNow },
                new User { Id = Guid.NewGuid(), FullName = "U3", Email = "u3@test.com", PasswordHash = "h", Role = UserRole.Admin, IsActive = false, CreatedAt = DateTime.UtcNow.AddMonths(-2) }
            );
            await _context.SaveChangesAsync();

            // Act
            var summary = await _service.GetUserSummaryAsync();

            // Assert
            Assert.Equal(3, summary.TotalUsers);
            Assert.Equal(2, summary.ActiveUsers);
            Assert.Equal(1, summary.InactiveUsers);
            Assert.Equal(2, summary.NewUsersThisMonth);
            Assert.Equal(3, summary.RoleBreakdown.Count);
        }

        [Fact]
        public async Task GetTeacherNameAndEmail_ShouldReturnTeacherDetails_WhenAuthorized()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var teacher = new User { Id = teacherId, FullName = "Prof John", Email = "john@school.com", PasswordHash = "h", Role = UserRole.Teacher };
            _context.Users.Add(teacher);
            await _context.SaveChangesAsync();

            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, teacherId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            }, "Test"));

            // Act
            var (name, email, id) = await _service.GetTeacherNameAndEmail(_httpContext.User, teacherId);

            // Assert
            Assert.Equal("Prof John", name);
            Assert.Equal("john@school.com", email);
            Assert.Equal(teacherId, id);
        }

        [Fact]
        public async Task GetTeacherNameAndEmail_ShouldThrowForbidden_WhenIdMismatchesClaims()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, teacherId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            }, "Test"));

            // Act & Assert
            await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetTeacherNameAndEmail(_httpContext.User, Guid.NewGuid()));
        }

        [Fact]
        public async Task GetTeacherNameAndEmail_ShouldThrowNotFound_WhenTeacherDoesNotExist()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, teacherId.ToString()),
                new Claim(ClaimTypes.Role, "Teacher")
            }, "Test"));

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _service.GetTeacherNameAndEmail(_httpContext.User, teacherId));
        }

        [Fact]
        public async Task GetUserIdAndEmailFromClaims_ShouldExtractClaims()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, "claims@test.com"),
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(ClaimTypes.Role, "Teacher")
            }, "Test"));

            // Act
            var (claimsUserId, claimsEmail, claimsRoles) = await _service.GetUserIdAndEmailFromClaims();

            // Assert
            Assert.Equal(userId, claimsUserId);
            Assert.Equal("claims@test.com", claimsEmail);
            Assert.Contains("Admin", claimsRoles);
            Assert.Contains("Teacher", claimsRoles);
        }
    }
}
