using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using Backend.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Tests.Helpers;

namespace Tests.DatabaseTests
{
    public class EntityRelationshipAndConstraintTests
    {
        private readonly AppDbContext _context;

        public EntityRelationshipAndConstraintTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        [Fact]
        public void UniqueIndexes_ShouldBeDefinedOnEntities()
        {
            // User Email unique index
            var userEntity = _context.Model.FindEntityType(typeof(User));
            var emailIndex = userEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(User.Email)));
            Assert.NotNull(emailIndex);
            Assert.True(emailIndex.IsUnique);

            // Subject Code unique index
            var subjectEntity = _context.Model.FindEntityType(typeof(Subject));
            var codeIndex = subjectEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Any(p => p.Name == nameof(Subject.Code)));
            Assert.NotNull(codeIndex);
            Assert.True(codeIndex.IsUnique);

            // ClassSubject (ClassId, SubjectId) unique index
            var csEntity = _context.Model.FindEntityType(typeof(ClassSubject));
            var csIndex = csEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Count == 2 &&
                i.Properties.Any(p => p.Name == nameof(ClassSubject.ClassId)) &&
                i.Properties.Any(p => p.Name == nameof(ClassSubject.SubjectId)));
            Assert.NotNull(csIndex);
            Assert.True(csIndex.IsUnique);

            // TeacherAssignment (TeacherId, ClassSubjectId) unique index
            var taEntity = _context.Model.FindEntityType(typeof(TeacherAssignment));
            var taIndex = taEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Count == 2 &&
                i.Properties.Any(p => p.Name == nameof(TeacherAssignment.TeacherId)) &&
                i.Properties.Any(p => p.Name == nameof(TeacherAssignment.ClassSubjectId)));
            Assert.NotNull(taIndex);
            Assert.True(taIndex.IsUnique);

            // StudentEnrollment (StudentId, ClassId) unique index
            var seEntity = _context.Model.FindEntityType(typeof(StudentEnrollment));
            var seIndex = seEntity?.GetIndexes().FirstOrDefault(i => i.Properties.Count == 2 &&
                i.Properties.Any(p => p.Name == nameof(StudentEnrollment.StudentId)) &&
                i.Properties.Any(p => p.Name == nameof(StudentEnrollment.ClassId)));
            Assert.NotNull(seIndex);
            Assert.True(seIndex.IsUnique);
        }

        [Fact]
        public void ForeignKeys_ShouldHaveRestrictDeleteBehavior_ForCoreDomainEntities()
        {
            var assignmentEntity = _context.Model.FindEntityType(typeof(Assignment));
            Assert.NotNull(assignmentEntity);

            foreach (var fk in assignmentEntity.GetForeignKeys())
            {
                Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
            }

            var submissionEntity = _context.Model.FindEntityType(typeof(Submission));
            Assert.NotNull(submissionEntity);

            foreach (var fk in submissionEntity.GetForeignKeys())
            {
                Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
            }

            var csEntity = _context.Model.FindEntityType(typeof(ClassSubject));
            Assert.NotNull(csEntity);

            foreach (var fk in csEntity.GetForeignKeys())
            {
                Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
            }
        }
    }
}
