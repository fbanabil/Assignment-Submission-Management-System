using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.HandlerTests.Admin
{
    public class DashboardHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly DashboardHandler _handler;

        public DashboardHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockSubmissionService = new Mock<ISubmissionService>();

            _handler = new DashboardHandler(
                _mockUserService.Object,
                _mockAssignmentService.Object,
                _mockSubmissionService.Object,
                Mock.Of<ILogger<DashboardHandler>>());
        }

        [Fact]
        public async Task HandleDashboardAsync_ShouldReturnOk_WithAggregatedStats()
        {
            // Arrange
            var userSummary = new UserSummaryDto { TotalUsers = 10 };
            var assignmentSummary = new AssignmentSummaryDto { TotalAssignments = 5 };
            var submissionSummary = new SubmissionSummaryDto { TotalSubmissions = 20 };

            _mockUserService.Setup(s => s.GetUserSummaryAsync()).ReturnsAsync(userSummary);
            _mockAssignmentService.Setup(s => s.GetAssignmentSummaryAsync()).ReturnsAsync(assignmentSummary);
            _mockSubmissionService.Setup(s => s.GetSubmissionSummaryAsync()).ReturnsAsync(submissionSummary);

            // Act
            var result = await _handler.HandleDashboardAsync();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<DashboardSummaryDto>(okResult.Value);
            Assert.Equal("Server", dto.DataSource);
            Assert.Equal(10, dto.Users.TotalUsers);
            Assert.Equal(5, dto.Assignments.TotalAssignments);
            Assert.Equal(20, dto.Submissions.TotalSubmissions);
        }
    }
}
