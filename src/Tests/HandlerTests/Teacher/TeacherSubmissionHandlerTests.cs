using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Teacher;
using Backend.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Teacher
{
    public class TeacherSubmissionHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly TeacherSubmissionHandler _handler;

        public TeacherSubmissionHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockSubmissionService = new Mock<ISubmissionService>();
            var (accessor, _) = MockHelper.CreateMockHttpContext(role: "Teacher");
            _mockHttpContextAccessor = accessor;

            _handler = new TeacherSubmissionHandler(
                _mockUserService.Object,
                _mockSubmissionService.Object,
                _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task HandleGetSubmissionsAsync_ShouldThrowBadRequest_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleGetSubmissionsAsync(null!));
        }

        [Fact]
        public async Task HandleGetSubmissionsAsync_ShouldReturnOk_WithSubmissions()
        {
            // Arrange
            var dto = new SubmissionFilterDto();
            var paged = new PagedResultDto<SubmissionResponseDto> { Items = new List<SubmissionResponseDto>(), TotalCount = 0 };
            _mockSubmissionService.Setup(s => s.GetSubmissionsAsync(dto)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetSubmissionsAsync(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleGradeSubmissionAsync_ShouldThrowBadRequest_WhenDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<BadRequestException>(() => _handler.HandleGradeSubmissionAsync(null!, Guid.NewGuid()));
        }

        [Fact]
        public async Task HandleGradeSubmissionAsync_ShouldReturnOk_WhenGradedSuccessfully()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var dto = new GradeDto { SubmissionId = Guid.NewGuid(), Marks = 90, Feedback = "Good work" };

            _mockUserService.Setup(u => u.GetTeacherNameAndEmail(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), teacherId))
                .ReturnsAsync(("Teacher", "t@test.com", teacherId));
            _mockSubmissionService.Setup(s => s.GradeSubmissionAsync(dto, teacherId)).Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleGradeSubmissionAsync(dto, teacherId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
            _mockSubmissionService.Verify(s => s.GradeSubmissionAsync(dto, teacherId), Times.Once);
        }
    }
}
