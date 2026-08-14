using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.StudentEnrollmentDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Teacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;

namespace Tests.ControllerTests
{
    public class TeacherControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly Mock<ITeacherAssignmentService> _mockTeacherAssignmentService;
        private readonly Mock<IStudentEnrollmentService> _mockStudentEnrollmentService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly DefaultHttpContext _httpContext;
        private readonly TeacherController _controller;

        public TeacherControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockSubmissionService = new Mock<ISubmissionService>();
            _mockTeacherAssignmentService = new Mock<ITeacherAssignmentService>();
            _mockStudentEnrollmentService = new Mock<IStudentEnrollmentService>();

            Guid teacherId = Guid.NewGuid();
            (_mockHttpContextAccessor, _httpContext) = MockHelper.CreateMockHttpContext(userId: teacherId, role: "Teacher");

            _mockUserService.Setup(u => u.GetTeacherNameAndEmail(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), It.IsAny<Guid?>()))
                .ReturnsAsync(("Teacher Name", "teacher@example.com", teacherId));

            var dashboardHandler = new TeacherDashboardHandler(
                _mockUserService.Object,
                _mockAssignmentService.Object,
                _mockSubmissionService.Object,
                _mockTeacherAssignmentService.Object,
                _mockHttpContextAccessor.Object);

            var classHandler = new TeacherClassHandler(
                _mockUserService.Object,
                _mockTeacherAssignmentService.Object,
                _mockHttpContextAccessor.Object);

            var assignmentHandler = new TeacherAssignmentHandler(_mockAssignmentService.Object);

            var submissionHandler = new TeacherSubmissionHandler(
                _mockUserService.Object,
                _mockSubmissionService.Object,
                _mockHttpContextAccessor.Object);

            var enrollmentHandler = new TeacherEnrollmentHandler(
                _mockUserService.Object,
                _mockStudentEnrollmentService.Object,
                _mockHttpContextAccessor.Object);

            _controller = new TeacherController(
                dashboardHandler,
                classHandler,
                assignmentHandler,
                submissionHandler,
                enrollmentHandler);
        }

        [Fact]
        public async Task Dashboard_ShouldReturnOk_WithDashboardData()
        {
            // Arrange
            var dto = new TeacherDashboardFilterDto();
            _mockAssignmentService.Setup(s => s.GetUpcomingDeadlines(It.IsAny<Guid>()))
                .ReturnsAsync(new List<TeacherUpcomingDeadlineDto>());

            // Act
            var result = await _controller.Dashboard(dto, Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<TeacherDashboardResponseDto>(okResult.Value);
            Assert.Equal("Teacher Name", response.TeacherName);
        }

        [Fact]
        public async Task Classes_ShouldReturnOk_WithAssignedClasses()
        {
            // Arrange
            var dto = new TeacherClassFilterDto();
            var paged = new PagedResultDto<TeacherAssignedClassSubjectDto> { Items = new List<TeacherAssignedClassSubjectDto>(), TotalCount = 0 };
            _mockTeacherAssignmentService.Setup(s => s.GetAssignedClassesPagedAsync(It.IsAny<Guid>(), dto))
                .ReturnsAsync(paged);

            // Act
            var result = await _controller.Classes(dto, Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Assignments_Get_ShouldReturnOk()
        {
            // Arrange
            var dto = new AssignmentFilterDto();
            var paged = new PagedResultDto<AssignmentResponseDto> { Items = new List<AssignmentResponseDto>(), TotalCount = 0 };
            _mockAssignmentService.Setup(s => s.GetAssignmentsForTeacher(dto)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Assignments(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Assignments_Post_ShouldReturnCreatedResult()
        {
            // Arrange
            var dto = new AssignmentCreateDto { Title = "Assignment 1" };
            var response = new AssignmentResponseDto { Id = Guid.NewGuid(), Title = dto.Title };
            _mockAssignmentService.Setup(s => s.CreateAssignmentAsync(dto)).ReturnsAsync(response);

            // Act
            var result = await _controller.Assignments(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
            Assert.Equal(response, objectResult.Value);
        }

        [Fact]
        public async Task UpdateAssignment_ShouldReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new AssignmentUpdateDto { Title = "Updated" };
            _mockAssignmentService.Setup(s => s.UpdateAssignmentAsync(id, dto)).ReturnsAsync(new AssignmentResponseDto());

            // Act
            var result = await _controller.UpdateAssignment(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task Submissions_Get_ShouldReturnOk()
        {
            // Arrange
            var dto = new SubmissionFilterDto();
            var paged = new PagedResultDto<SubmissionResponseDto> { Items = new List<SubmissionResponseDto>(), TotalCount = 0 };
            _mockSubmissionService.Setup(s => s.GetSubmissionsAsync(dto)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Submissions(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task GradeSubmission_ShouldReturnOk()
        {
            // Arrange
            var dto = new GradeDto { SubmissionId = Guid.NewGuid(), Marks = 95, Feedback = "Excellent" };
            _mockSubmissionService.Setup(s => s.GradeSubmissionAsync(dto, It.IsAny<Guid>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.GradeSubmission(dto, Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Enrollments_Get_ShouldReturnOk()
        {
            // Arrange
            var dto = new StudentEnrollmentFilterDto();
            var paged = new PagedResultDto<StudentEnrollmentResponseDto> { Items = new List<StudentEnrollmentResponseDto>(), TotalCount = 0 };
            _mockStudentEnrollmentService.Setup(s => s.GetStudentEnrollmentsForTeacherAsync(It.IsAny<Guid>(), dto)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Enrollments(dto, Guid.Empty);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Enrollments_Post_ShouldReturnCreatedStatus()
        {
            // Arrange
            var dto = new StudentEnrollmentCreateDto { ClassId = Guid.NewGuid(), StudentEmail = "student@example.com" };
            var enrollment = new StudentEnrollment { Id = Guid.NewGuid(), ClassId = dto.ClassId };
            _mockStudentEnrollmentService.Setup(s => s.CreateStudentEnrollmentAsync(dto)).ReturnsAsync(enrollment);

            // Act
            var result = await _controller.Enrollments(dto);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        }

        [Fact]
        public async Task DeleteEnrollment_ShouldReturnNoContent()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockStudentEnrollmentService.Setup(s => s.DeleteStudentEnrollmentAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteEnrollment(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
