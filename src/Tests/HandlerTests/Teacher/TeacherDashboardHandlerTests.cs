using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs.TeacherDTOs;
using Backend.Handlers.Teacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Teacher
{
    public class TeacherDashboardHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly Mock<ITeacherAssignmentService> _mockTeacherAssignmentService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly TeacherDashboardHandler _handler;

        public TeacherDashboardHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockSubmissionService = new Mock<ISubmissionService>();
            _mockTeacherAssignmentService = new Mock<ITeacherAssignmentService>();
            var (accessor, _) = MockHelper.CreateMockHttpContext(role: "Teacher");
            _mockHttpContextAccessor = accessor;

            _handler = new TeacherDashboardHandler(
                _mockUserService.Object,
                _mockAssignmentService.Object,
                _mockSubmissionService.Object,
                _mockTeacherAssignmentService.Object,
                _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task HandleDashboardAsync_ShouldReturnOk_WithDashboardSummary()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var dto = new TeacherDashboardFilterDto();

            _mockUserService.Setup(u => u.GetTeacherNameAndEmail(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), teacherId))
                .ReturnsAsync(("Teacher Name", "teacher@test.com", teacherId));
            _mockAssignmentService.Setup(s => s.GetTotalAssignedClassesCount(teacherId)).ReturnsAsync(3);
            _mockSubmissionService.Setup(s => s.GetUngradedSubmissionsCount(teacherId)).ReturnsAsync(5);
            _mockAssignmentService.Setup(s => s.GetActiveAssignmentsCount(teacherId)).ReturnsAsync(2);
            _mockTeacherAssignmentService.Setup(s => s.GetAssignedClasses(teacherId))
                .ReturnsAsync(new List<TeacherAssignedClassSubjectDto>());
            _mockAssignmentService.Setup(s => s.GetUpcomingDeadlines(teacherId))
                .ReturnsAsync(new List<TeacherUpcomingDeadlineDto>());

            // Act
            var result = await _handler.HandleDashboardAsync(dto, teacherId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TeacherDashboardResponseDto>(okResult.Value);
            Assert.Equal("Teacher Name", response.TeacherName);
            Assert.Equal(3, response.TotalAssignedClasses);
            Assert.Equal(5, response.UngradedSubmissionsCount);
            Assert.Equal(2, response.ActiveAssignmentsCount);
        }
    }
}
