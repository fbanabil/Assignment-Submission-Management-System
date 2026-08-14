using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.StudentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Student;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Student
{
    public class StudentAssignmentHandlerTests
    {
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly AppDbContext _context;
        private readonly StudentAssignmentHandler _handler;
        private readonly Guid _studentId;

        public StudentAssignmentHandlerTests()
        {
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockSubmissionService = new Mock<ISubmissionService>();
            _mockUserService = new Mock<IUserService>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _context = TestDbContextFactory.CreateInMemoryDbContext();

            _studentId = Guid.NewGuid();
            var student = new User { Id = _studentId, FullName = "Student 1", Email = "s1@test.com", PasswordHash = "h", Role = UserRole.Student };
            _context.Users.Add(student);
            _context.SaveChanges();

            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((_studentId, "s1@test.com", new List<string> { "Student" }));

            _handler = new StudentAssignmentHandler(
                _mockAssignmentService.Object,
                _mockSubmissionService.Object,
                _mockUserService.Object,
                _context,
                Mock.Of<ILogger<StudentAssignmentHandler>>());
        }

        [Fact]
        public async Task HandleGetStudentAssignmentsAsync_ShouldReturnOk()
        {
            // Arrange
            var filter = new StudentAssignmentFilterDto();
            var paged = new PagedResultDto<StudentAssignmentResponseDto> { Items = new List<StudentAssignmentResponseDto>(), TotalCount = 0 };
            _mockAssignmentService.Setup(s => s.GetAssignmentsForStudentPagedAsync(_studentId, filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetStudentAssignmentsAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task HandleGetStudentAssignmentDetailAsync_ShouldReturnNotFound_WhenNull()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            _mockAssignmentService.Setup(s => s.GetAssignmentDetailForStudentAsync(_studentId, assignmentId))
                .ReturnsAsync((StudentAssignmentDetailDto?)null);

            // Act
            var result = await _handler.HandleGetStudentAssignmentDetailAsync(assignmentId);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFound.Value);
        }

        [Fact]
        public async Task HandleGetStudentAssignmentDetailAsync_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var assignmentId = Guid.NewGuid();
            var detail = new StudentAssignmentDetailDto { Id = assignmentId, Title = "Assignment 1" };
            _mockAssignmentService.Setup(s => s.GetAssignmentDetailForStudentAsync(_studentId, assignmentId))
                .ReturnsAsync(detail);

            // Act
            var result = await _handler.HandleGetStudentAssignmentDetailAsync(assignmentId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(detail, okResult.Value);
        }

        [Fact]
        public async Task HandleCreateStudentSubmissionAsync_ShouldReturnStatusCode201()
        {
            // Arrange
            var dto = new StudentSubmissionCreateDto { AssignmentId = Guid.NewGuid(), SubmissionText = "Submission" };
            _mockSubmissionService.Setup(s => s.CreateStudentSubmissionAsync(_studentId, dto))
                .ReturnsAsync(new StudentSubmissionDetailDto());

            // Act
            var result = await _handler.HandleCreateStudentSubmissionAsync(dto);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
        }

        [Fact]
        public async Task HandleFileUploadAsync_ShouldReturnOk_WithUploadDetails()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns("F:/wwwroot");
            var response = new FileUploadResponseDto { FilePath = "/assignments/file.pdf" };
            _mockSubmissionService.Setup(s => s.UploadAssignmentFileAsync(mockFile.Object, "F:/wwwroot"))
                .ReturnsAsync(response);

            // Act
            var result = await _handler.HandleFileUploadAsync(mockFile.Object, _mockEnvironment.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(response, okResult.Value);
        }

        [Fact]
        public async Task HandleUnsubmitAssignmentAsync_ShouldReturnNoContent()
        {
            // Arrange
            var subId = Guid.NewGuid();
            _mockSubmissionService.Setup(s => s.UnsubmitAssignmentAsync(_studentId, subId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.HandleUnsubmitAssignmentAsync(subId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubmissionService.Verify(s => s.UnsubmitAssignmentAsync(_studentId, subId), Times.Once);
        }

        [Fact]
        public async Task HandleGetStudentSubmissionsHistoryAsync_ShouldReturnOk()
        {
            // Arrange
            var filter = new StudentSubmissionHistoryFilterDto();
            var paged = new PagedResultDto<StudentSubmissionHistoryResponseDto> { Items = new List<StudentSubmissionHistoryResponseDto>(), TotalCount = 0 };
            _mockSubmissionService.Setup(s => s.GetStudentSubmissionHistoryPagedAsync(_studentId, filter)).ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetStudentSubmissionsHistoryAsync(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
