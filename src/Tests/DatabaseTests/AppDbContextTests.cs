using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.DatabaseTests
{
    public class AppDbContextTests
    {
        private readonly AppDbContext _context;

        public AppDbContextTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        [Fact]
        public void DbSets_ShouldBeInitializedAndNotNull()
        {
            Assert.NotNull(_context.Users);
            Assert.NotNull(_context.Classes);
            Assert.NotNull(_context.Subjects);
            Assert.NotNull(_context.ClassSubjects);
            Assert.NotNull(_context.TeacherAssignments);
            Assert.NotNull(_context.StudentEnrollments);
            Assert.NotNull(_context.Assignments);
            Assert.NotNull(_context.Submissions);
            Assert.NotNull(_context.RefreshTokens);
        }

        [Fact]
        public void ModelConfiguration_UserEntity_ShouldHaveRequiredFieldsAndMaxLengths()
        {
            var entityType = _context.Model.FindEntityType(typeof(User));
            Assert.NotNull(entityType);

            var emailProp = entityType.FindProperty(nameof(User.Email));
            Assert.NotNull(emailProp);
            Assert.False(emailProp.IsNullable);
            Assert.Equal(255, emailProp.GetMaxLength());

            var nameProp = entityType.FindProperty(nameof(User.FullName));
            Assert.NotNull(nameProp);
            Assert.False(nameProp.IsNullable);
            Assert.Equal(255, nameProp.GetMaxLength());

            var passProp = entityType.FindProperty(nameof(User.PasswordHash));
            Assert.NotNull(passProp);
            Assert.False(passProp.IsNullable);
            Assert.Equal(255, passProp.GetMaxLength());

            var roleProp = entityType.FindProperty(nameof(User.Role));
            Assert.NotNull(roleProp);
            Assert.Equal(typeof(string), roleProp.GetProviderClrType());
        }

        [Fact]
        public void ModelConfiguration_AssignmentEntity_ShouldHaveRequiredConstraints()
        {
            var entityType = _context.Model.FindEntityType(typeof(Assignment));
            Assert.NotNull(entityType);

            var titleProp = entityType.FindProperty(nameof(Assignment.Title));
            Assert.NotNull(titleProp);
            Assert.False(titleProp.IsNullable);
            Assert.Equal(200, titleProp.GetMaxLength());

            var descProp = entityType.FindProperty(nameof(Assignment.Description));
            Assert.NotNull(descProp);
            Assert.False(descProp.IsNullable);
            Assert.Equal(2000, descProp.GetMaxLength());

            var statusProp = entityType.FindProperty(nameof(Assignment.Status));
            Assert.NotNull(statusProp);
            Assert.Equal(typeof(string), statusProp.GetProviderClrType());
        }

        [Fact]
        public void ModelConfiguration_SubmissionEntity_ShouldHaveConstraints()
        {
            var entityType = _context.Model.FindEntityType(typeof(Submission));
            Assert.NotNull(entityType);

            var textProp = entityType.FindProperty(nameof(Submission.SubmissionText));
            Assert.NotNull(textProp);
            Assert.Equal(4000, textProp.GetMaxLength());

            var fileUrlProp = entityType.FindProperty(nameof(Submission.FileUrl));
            Assert.NotNull(fileUrlProp);
            Assert.Equal(500, fileUrlProp.GetMaxLength());

            var feedbackProp = entityType.FindProperty(nameof(Submission.Feedback));
            Assert.NotNull(feedbackProp);
            Assert.Equal(1000, feedbackProp.GetMaxLength());

            var statusProp = entityType.FindProperty(nameof(Submission.Status));
            Assert.NotNull(statusProp);
            Assert.Equal(typeof(string), statusProp.GetProviderClrType());
        }

        [Fact]
        public void ModelConfiguration_RefreshTokenEntity_ShouldHaveCascadeDelete()
        {
            var entityType = _context.Model.FindEntityType(typeof(RefreshToken));
            Assert.NotNull(entityType);

            var userFk = entityType.GetForeignKeys().FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(User));
            Assert.NotNull(userFk);
            Assert.Equal(DeleteBehavior.Cascade, userFk.DeleteBehavior);
        }
    }
}
