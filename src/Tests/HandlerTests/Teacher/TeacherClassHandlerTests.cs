using AssignmentSystem.Api.Services.Interfaces;
using Backend.DTOs;
using Backend.DTOs.TeacherDTOs;
using Backend.DTOs.UserDTOs;
using Backend.Handlers.Teacher;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Tests.Helpers;

namespace Tests.HandlerTests.Teacher
{
    public class TeacherClassHandlerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<ITeacherAssignmentService> _mockTeacherAssignmentService;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly TeacherClassHandler _handler;

        public TeacherClassHandlerTests()
        {
            _mockUserService = new Mock<IUserService>();
            _mockTeacherAssignmentService = new Mock<ITeacherAssignmentService>();
            var (accessor, _) = MockHelper.CreateMockHttpContext(role: "Teacher");
            _mockHttpContextAccessor = accessor;

            _handler = new TeacherClassHandler(
                _mockUserService.Object,
                _mockTeacherAssignmentService.Object,
                _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task HandleGetClassesAsync_ShouldReturnOk_WithAssignedClasses()
        {
            // Arrange
            var teacherId = Guid.NewGuid();
            var dto = new TeacherClassFilterDto();
            var paged = new PagedResultDto<TeacherAssignedClassSubjectDto> { Items = new List<TeacherAssignedClassSubjectDto>(), TotalCount = 0 };

            _mockUserService.Setup(u => u.GetTeacherNameAndEmail(It.IsAny<System.Security.Claims.ClaimsPrincipal>(), teacherId))
                .ReturnsAsync(("Teacher", "teacher@test.com", teacherId));
            _mockTeacherAssignmentService.Setup(s => s.GetAssignedClassesPagedAsync(teacherId, dto))
                .ReturnsAsync(paged);

            // Act
            var result = await _handler.HandleGetClassesAsync(dto, teacherId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(paged, okResult.Value);
        }
    }
}
