using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Teacher;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Teacher
{
    public class TeacherEnrollmentHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IStudentEnrollmentService> _mockStudentEnrollmentService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly TeacherEnrollmentHandler _handler;

        public TeacherEnrollmentHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockStudentEnrollmentService = new Mock<IStudentEnrollmentService>();
            var (accessor, _) = MockHelper.CreateMockHttpContext(role: "Teacher");
            _mockHttpContextAccessor = accessor;

            _handler = new TeacherEnrollmentHandler(
                _mockUserService.Object,
                _mockStudentEnrollmentService.Object,
                _mockHttpContextAccessor.Object,
                Mock.Of<ILogger<TeacherEnrollmentHandler>>());
        }

        [Fact]
        public async Task HandleGetEnrollmentsAsync_ShouldReturnOk_WithEnrollments()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var dto = new StudentEnrollmentFilterDto();
            var paged = new PagedResultDto<StudentEnrollmentResponseDto> { Items = new List<StudentEnrollmentResponseDto>(), TotalCount = 0 };

            _mockUserService.Setup(u => u.GetTeacherNameAndEmail(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), teacherId))
                .ReturnsAsync(("Teacher", "t@test.com", teacherId));
            _mockStudentEnrollmentService.Setup(s => s.GetStudentEnrollmentsForTeacherAsync(teacherId, dto))
                .ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetEnrollmentsAsync(dto, teacherId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleCreateEnrollmentAsync_ShouldThrowBadRequest_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleCreateEnrollmentAsync(null!));
        }

        [Fact]
        public async Task HandleCreateEnrollmentAsync_ShouldReturnCreatedStatus_WhenValid()
        {
            // Arrange
            var dto = new StudentEnrollmentCreateDto { ClassId = Guid.NewGuid(), StudentEmail = "student@test.com" };
            var created = new StudentEnrollment { Id = Guid.NewGuid(), ClassId = dto.ClassId };
            _mockStudentEnrollmentService.Setup(s => s.CreateStudentEnrollmentAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _handler.HandleCreateEnrollmentAsync(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal(created, objectResult.Value);
        }

        [Fact]
        public async Task HandleDeleteEnrollmentAsync_ShouldReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockStudentEnrollmentService.Setup(s => s.DeleteStudentEnrollmentAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleDeleteEnrollmentAsync(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockStudentEnrollmentService.Verify(s => s.DeleteStudentEnrollmentAsync(id), Times.Once);
        }
    }
}
