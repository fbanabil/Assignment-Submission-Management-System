using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Services.Interfaces;
using Backend.Controllers;
using Backend.DTOs;
using Backend.DTOs.AssignmentDTOs;
using Backend.DTOs.ClassDTOs;
using Backend.DTOs.ClassSubjectDTOs;
using Backend.DTOs.SubjectDTOs;
using Backend.DTOs.SubmissionDTOs;
using Backend.DTOs.TeacherAssignmentDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Admin;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Tests.ControllerTests
{
    public class AdminControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IAssignmentService> _mockAssignmentService;
        private readonly Mock<ISubmissionService> _mockSubmissionService;
        private readonly Mock<IClassService> _mockClassService;
        private readonly Mock<ISubjectService> _mockSubjectService;
        private readonly Mock<IClassSubjectService> _mockClassSubjectService;
        private readonly Mock<ITeacherAssignmentService> _mockTeacherAssignmentService;

        private readonly AdminController _controller;

        public AdminControllerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockAssignmentService = new Mock<IAssignmentService>();
            _mockSubmissionService = new Mock<ISubmissionService>();
            _mockClassService = new Mock<IClassService>();
            _mockSubjectService = new Mock<ISubjectService>();
            _mockClassSubjectService = new Mock<IClassSubjectService>();
            _mockTeacherAssignmentService = new Mock<ITeacherAssignmentService>();

            var dashboardHandler = new DashboardHandler(_mockUserService.Object, _mockAssignmentService.Object, _mockSubmissionService.Object);
            var userHandler = new UserHandler(_mockUserService.Object);
            var classHandler = new ClassHandler(_mockClassService.Object);
            var subjectHandler = new SubjectHandler(_mockSubjectService.Object);
            var classSubjectHandler = new ClassSubjectHandler(_mockClassSubjectService.Object);
            var teacherAssignmentHandler = new TeacherAssignmentHandler(_mockTeacherAssignmentService.Object);
            var assignmentHandler = new AssignmentHandler(_mockAssignmentService.Object);
            var submissionHandler = new SubmissionHandler(_mockSubmissionService.Object);

            _controller = new AdminController(
                dashboardHandler,
                userHandler,
                classHandler,
                subjectHandler,
                classSubjectHandler,
                teacherAssignmentHandler,
                assignmentHandler,
                submissionHandler);
        }

        [Fact]
        public async Task Dashboard_ShouldReturnOkResult_WithSummaryDto()
        {
            // Arrange
            _mockUserService.Setup(s => s.GetUserSummaryAsync()).ReturnsAsync(new UserSummaryDto());
            _mockAssignmentService.Setup(s => s.GetAssignmentSummaryAsync()).ReturnsAsync(new AssignmentSummaryDto());
            _mockSubmissionService.Setup(s => s.GetSubmissionSummaryAsync()).ReturnsAsync(new SubmissionSummaryDto());

            // Act
            var result = await _controller.Dashboard();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var dto = Assert.IsType<DashboardSummaryDto>(okResult.Value);
            Assert.Equal("Server", dto.DataSource);
        }

        [Fact]
        public async Task Users_ShouldReturnOkResult_WithPagedUsers()
        {
            // Arrange
            var filter = new UserFilterDto();
            var paged = new PagedResultDto<UserResponseDto> { Items = new List<UserResponseDto>(), TotalCount = 0 };
            _mockUserService.Setup(s => s.GetUsersAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Users(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Classes_Get_ShouldReturnOkResult_WithPagedClasses()
        {
            // Arrange
            var filter = new ClassFilterDto();
            var paged = new PagedResultDto<ClassResponseDto> { Items = new List<ClassResponseDto>(), TotalCount = 0 };
            _mockClassService.Setup(s => s.GetClassesAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Classes(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Classes_Post_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var dto = new ClassCreateDto { Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var created = new Class { Id = Guid.NewGuid(), Name = dto.Name, Section = dto.Section, AcademicYear = dto.AcademicYear };
            _mockClassService.Setup(s => s.CreateClassAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _controller.Classes(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AdminController.Classes), createdResult.ActionName);
        }

        [Fact]
        public async Task Classes_Put_ShouldReturnNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new ClassUpdateDto { Name = "Updated Class" };
            _mockClassService.Setup(s => s.UpdateClassAsync(id, dto)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Classes(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockClassService.Verify(s => s.UpdateClassAsync(id, dto), Times.Once);
        }

        [Fact]
        public async Task DeleteClass_ShouldReturnNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockClassService.Setup(s => s.DeleteClassAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteClass(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockClassService.Verify(s => s.DeleteClassAsync(id), Times.Once);
        }

        [Fact]
        public async Task Subjects_Get_ShouldReturnOkResult_WithPagedSubjects()
        {
            // Arrange
            var filter = new SubjectFilterDto();
            var paged = new PagedResultDto<SubjectResponseDto> { Items = new List<SubjectResponseDto>(), TotalCount = 0 };
            _mockSubjectService.Setup(s => s.GetSubjectsAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Subjects(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Subjects_Post_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var dto = new SubjectCreateDto { Name = "Math", Code = "M101" };
            var created = new Subject { Id = Guid.NewGuid(), Name = dto.Name, Code = dto.Code };
            _mockSubjectService.Setup(s => s.CreateSubjectAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _controller.Subjects(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
            Assert.Equal(nameof(AdminController.Subjects), createdResult.ActionName);
        }

        [Fact]
        public async Task Subjects_Put_ShouldReturnNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new SubjectUpdateDto { Name = "Advanced Math" };
            _mockSubjectService.Setup(s => s.UpdateSubjectAsync(id, dto)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Subjects(id, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubjectService.Verify(s => s.UpdateSubjectAsync(id, dto), Times.Once);
        }

        [Fact]
        public async Task DeleteSubject_ShouldReturnNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockSubjectService.Setup(s => s.DeleteSubjectAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteSubject(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockSubjectService.Verify(s => s.DeleteSubjectAsync(id), Times.Once);
        }

        [Fact]
        public async Task ClassSubjects_Post_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var dto = new ClassSubjectCreateDto { ClassId = Guid.NewGuid(), SubjectId = Guid.NewGuid() };
            var created = new ClassSubject { Id = Guid.NewGuid(), ClassId = dto.ClassId, SubjectId = dto.SubjectId };
            _mockClassSubjectService.Setup(s => s.CreateClassSubjectAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _controller.ClassSubjects(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
        }

        [Fact]
        public async Task DeleteClassSubject_ShouldReturnNoContentResult_WhenFound()
        {
            // Arrange
            var classId = Guid.NewGuid();
            var subjectId = Guid.NewGuid();
            var csId = Guid.NewGuid();
            _mockClassSubjectService.Setup(s => s.GetAllClassSubjectsAsync())
                .ReturnsAsync(new List<ClassSubject> { new ClassSubject { Id = csId, ClassId = classId, SubjectId = subjectId } });
            _mockClassSubjectService.Setup(s => s.DeleteClassSubjectAsync(csId)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteClassSubject(classId, subjectId);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockClassSubjectService.Verify(s => s.DeleteClassSubjectAsync(csId), Times.Once);
        }

        [Fact]
        public async Task TeacherAssignments_Get_ShouldReturnOkResult()
        {
            // Arrange
            var dto = new TeacherAssignmentFilterDto();
            var paged = new PagedResultDto<TeacherAssignmentResponseDto> { Items = new List<TeacherAssignmentResponseDto>(), TotalCount = 0 };
            _mockTeacherAssignmentService.Setup(s => s.GetTeacherAssignmentsAsync(dto)).ReturnsAsync(paged);

            // Act
            var result = await _controller.TeacherAssignments(dto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task TeacherAssignments_Post_ShouldReturnCreatedAtActionResult()
        {
            // Arrange
            var dto = new TeacherAssignmentCreateDto { TeacherId = Guid.NewGuid(), ClassSubjectId = Guid.NewGuid() };
            var created = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = dto.TeacherId, ClassSubjectId = dto.ClassSubjectId };
            _mockTeacherAssignmentService.Setup(s => s.CreateTeacherAssignmentAsync(dto)).ReturnsAsync(created);

            // Act
            var result = await _controller.TeacherAssignments(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(created, createdResult.Value);
        }

        [Fact]
        public async Task DeleteTeacherAssignment_ShouldReturnNoContentResult()
        {
            // Arrange
            var id = Guid.NewGuid();
            _mockTeacherAssignmentService.Setup(s => s.DeleteTeacherAssignmentAsync(id)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteTeacherAssignment(id);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockTeacherAssignmentService.Verify(s => s.DeleteTeacherAssignmentAsync(id), Times.Once);
        }

        [Fact]
        public async Task Assignments_Get_ShouldReturnOkResult()
        {
            // Arrange
            var filter = new AssignmentFilterDto();
            var paged = new PagedResultDto<AssignmentResponseDto> { Items = new List<AssignmentResponseDto>(), TotalCount = 0 };
            _mockAssignmentService.Setup(s => s.GetAssignmentsAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Assignments(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }

        [Fact]
        public async Task Submissions_Get_ShouldReturnOkResult()
        {
            // Arrange
            var filter = new SubmissionFilterDto();
            var paged = new PagedResultDto<SubmissionResponseDto> { Items = new List<SubmissionResponseDto>(), TotalCount = 0 };
            _mockSubmissionService.Setup(s => s.GetSubmissionsAsync(filter)).ReturnsAsync(paged);

            // Act
            var result = await _controller.Submissions(filter);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
