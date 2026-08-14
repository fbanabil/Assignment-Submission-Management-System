using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.StudentDTOs;
using Backend.Handlers.Student;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;

namespace Tests.ControllerTests
{
    public class StudentControllerTests
    {
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;
        private readonly AppDbContext _context;
        private readonly StudentController _controller;

        public StudentControllerTests()
        {
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockSubmissionService = new Mock<ISubmissionService>();
            _mockUserService = new Mock<IUserService>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
            _context = TestDbContextFactory.CreateInMemoryDbContext();

            var studentUser = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Student User",
                Email = "student@example.com",
                PasswordHash = "hash",
                Role = UserRole.Student
            };
            _context.Users.Add(studentUser);
            _context.SaveChanges();

            _mockUserService.Setup(u => u.GetUserIdAndEmailFromClaims())
                .ReturnsAsync((studentUser.Id, studentUser.Email, new List<string> { "Student" }));

            var dashboardHandler = new StudentDashboardHandler(_context, _mockUserService.Object);
            var assignmentHandler = new StudentAssignmentHandler(_mockAssignmentService.Object, _mockSubmissionService.Object, _mockUserService.Object, _context);

            _controller = new StudentController(dashboardHandler, assignmentHandler, _mockEnvironment.Object);
        }

        [Fact]
        public async Task Dashboard_ShouldReturnOk_WithStudentDashboardData()
        {
            // Act
            var result = await _controller.Dashboard(null);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<StudentDashboardResponseDto>(okResult.Value);
            Assert.Equal("Student User", dto.StudentName);
        }

        [Fact]
        public async Task Assignments_ShouldReturnOk_WithStudentAssignments()
        {
            // Arrange
            var filter = new StudentAssignmentFilterDto();
            var paged = new PagedResultDto<StudentAssignmentResponseDto> { Items = new List<StudentAssignmentResponseDto>(), TotalCount = 0 };
            _mockAssignmentService.Setup(s => s.GetAssignmentsForStudentPagedAsync(It.IsAny<Guid>(), filter))
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.Assignments(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task AssignmentDetail_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var id = Guid.NewGuid();
            var detail = new StudentAssignmentDetailDto { Id = id, Title = "Homework 1" };
            _mockAssignmentService.Setup(s => s.GetAssignmentDetailForStudentAsync(It.IsAny<Guid>(), id))
                .ReturnsAsync(detail);

            // Act
            var result = await _controller.AssignmentDetail(id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(detail, okResult.Value);
        }

        [Fact]
        public async Task Submissions_ShouldReturnCreatedStatus()
        {
            // Arrange
            var dto = new StudentSubmissionCreateDto { AssignmentId = Guid.NewGuid(), SubmissionText = "My answer" };
            var detail = new StudentSubmissionDetailDto { SubmissionId = Guid.NewGuid(), SubmissionText = dto.SubmissionText };
            _mockSubmissionService.Setup(s => s.CreateStudentSubmissionAsync(It.IsAny<Guid>(), dto))
                .ReturnsAsync(detail);

            // Act
            var result = await _controller.Submissions(dto);

            // Assert
            var statusResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status201Created, statusResult.StatusCode);
        }

        [Fact]
        public async Task FileUpload_ShouldReturnOk_WithUploadedFileInfo()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            _mockEnvironment.Setup(e => e.WebRootPath).Returns("F:/mockroot");
            var uploadResponse = new FileUploadResponseDto { FilePath = "/assignments/test.pdf", OriginalFileName = "test.pdf" };
            _mockSubmissionService.Setup(s => s.UploadAssignmentFileAsync(mockFile.Object, "F:/mockroot"))
                .ReturnsAsync(uploadResponse);

            // Act
            var result = await _controller.FileUpload(mockFile.Object);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(uploadResponse, okResult.Value);
        }

        [Fact]
        public async Task Unsubmit_ShouldReturnNoContent()
        {
            // Arrange
            var submissionId = Guid.NewGuid();
            _mockSubmissionService.Setup(s => s.UnsubmitAssignmentAsync(It.IsAny<Guid>(), submissionId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Unsubmit(submissionId);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task MySubmissions_ShouldReturnOk_WithHistory()
        {
            // Arrange
            var filter = new StudentSubmissionHistoryFilterDto();
            var paged = new PagedResultDto<StudentSubmissionHistoryResponseDto> { Items = new List<StudentSubmissionHistoryResponseDto>(), TotalCount = 0 };
            _mockSubmissionService.Setup(s => s.GetStudentSubmissionHistoryPagedAsync(It.IsAny<Guid>(), filter))
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.MySubmissions(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
