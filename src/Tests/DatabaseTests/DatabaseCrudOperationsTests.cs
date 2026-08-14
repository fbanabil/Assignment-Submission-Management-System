using AssignmentSystem.Api.Data;
using AssignmentSystem.Api.Models.Entities;
using AssignmentSystem.Api.Models.Enums;
using Backend.Models.Entities;
using Tests.Helpers;

namespace Tests.DatabaseTests
{
    public class DatabaseCrudOperationsTests
    {
        private readonly AppDbContext _context;

        public DatabaseCrudOperationsTests()
        {
            _context = TestDbContextFactory.CreateInMemoryDbContext();
        }

        [Fact]
        public async Task User_CrudOperations_ShouldWorkCorrectly()
        {
            // Create
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = "Test User",
                Email = "test@domain.com",
                PasswordHash = "hash123",
                Role = UserRole.Student,
                RollNo = "ST-100",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Read
            var retrieved = await _context.Users.FindAsync(user.Id);
            Assert.NotNull(retrieved);
            Assert.Equal("Test User", retrieved.FullName);

            // Update
            retrieved.FullName = "Updated User";
            await _context.SaveChangesAsync();

            var updated = await _context.Users.FindAsync(user.Id);
            Assert.Equal("Updated User", updated!.FullName);

            // Delete
            _context.Users.Remove(updated);
            await _context.SaveChangesAsync();

            var deleted = await _context.Users.FindAsync(user.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task ClassAndSubject_CrudOperations_ShouldWorkCorrectly()
        {
            // Create Class
            var cls = new Class
            {
                Id = Guid.NewGuid(),
                Name = "Grade 10",
                Section = "B",
                AcademicYear = "2026",
                CreatedAt = DateTime.UtcNow
            };
            _context.Classes.Add(cls);

            // Create Subject
            var sub = new Subject
            {
                Id = Guid.NewGuid(),
                Name = "Computer Science",
                Code = "CS101"
            };
            _context.Subjects.Add(sub);
            await _context.SaveChangesAsync();

            // Read
            Assert.NotNull(await _context.Classes.FindAsync(cls.Id));
            Assert.NotNull(await _context.Subjects.FindAsync(sub.Id));

            // Link via ClassSubject
            var cs = new ClassSubject
            {
                Id = Guid.NewGuid(),
                ClassId = cls.Id,
                SubjectId = sub.Id
            };
            _context.ClassSubjects.Add(cs);
            await _context.SaveChangesAsync();

            Assert.NotNull(await _context.ClassSubjects.FindAsync(cs.Id));
        }

        [Fact]
        public async Task TeacherAssignment_AndStudentEnrollment_ShouldSaveProperly()
        {
            var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", PasswordHash = "h", Role = UserRole.Teacher };
            var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", PasswordHash = "h", Role = UserRole.Student };
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Math", Code = "MTH" };
            var cs = new ClassSubject { Id = Guid.NewGuid(), ClassId = cls.Id, SubjectId = sub.Id };

            _context.Users.AddRange(teacher, student);
            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            _context.ClassSubjects.Add(cs);
            await _context.SaveChangesAsync();

            var ta = new TeacherAssignment { Id = Guid.NewGuid(), TeacherId = teacher.Id, ClassSubjectId = cs.Id };
            var se = new StudentEnrollment { Id = Guid.NewGuid(), StudentId = student.Id, ClassId = cls.Id, EnrolledAt = DateTime.UtcNow };

            _context.TeacherAssignments.Add(ta);
            _context.StudentEnrollments.Add(se);
            await _context.SaveChangesAsync();

            Assert.NotNull(await _context.TeacherAssignments.FindAsync(ta.Id));
            Assert.NotNull(await _context.StudentEnrollments.FindAsync(se.Id));
        }

        [Fact]
        public async Task Assignment_AndSubmission_CrudOperations_ShouldWorkCorrectly()
        {
            var teacher = new User { Id = Guid.NewGuid(), FullName = "Teacher", Email = "t@test.com", PasswordHash = "h", Role = UserRole.Teacher };
            var student = new User { Id = Guid.NewGuid(), FullName = "Student", Email = "s@test.com", PasswordHash = "h", Role = UserRole.Student };
            var cls = new Class { Id = Guid.NewGuid(), Name = "Class 10", Section = "A", AcademicYear = "2026" };
            var sub = new Subject { Id = Guid.NewGuid(), Name = "Math", Code = "MTH" };

            _context.Users.AddRange(teacher, student);
            _context.Classes.Add(cls);
            _context.Subjects.Add(sub);
            await _context.SaveChangesAsync();

            // Create Assignment
            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                Title = "Math Quiz 1",
                Description = "Algebra problems",
                ClassId = cls.Id,
                SubjectId = sub.Id,
                TeacherId = teacher.Id,
                Deadline = DateTime.UtcNow.AddDays(7),
                MaxMarks = 50,
                Status = AssignmentStatus.Published,
                AllowLateSubmission = true,
                AllowResubmission = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Assignments.Add(assignment);
            await _context.SaveChangesAsync();

            // Create Submission
            var submission = new Submission
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignment.Id,
                StudentId = student.Id,
                SubmissionText = "Completed Quiz",
                SubmittedAt = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted
            };
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();

            // Read Submission with includes
            var savedSub = await _context.Submissions
                .Include(s => s.Assignment)
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.Id == submission.Id);

            Assert.NotNull(savedSub);
            Assert.Equal("Math Quiz 1", savedSub.Assignment.Title);
            Assert.Equal("Student", savedSub.Student.FullName);

            // Grade Submission
            savedSub.Marks = 48;
            savedSub.Feedback = "Great job!";
            savedSub.GradedBy = teacher.Id;
            savedSub.GradedAt = DateTime.UtcNow;
            savedSub.Status = SubmissionStatus.Graded;
            await _context.SaveChangesAsync();

            var gradedSub = await _context.Submissions.FindAsync(submission.Id);
            Assert.Equal(48, gradedSub!.Marks);
            Assert.Equal(SubmissionStatus.Graded, gradedSub.Status);
        }

        [Fact]
        public async Task RefreshToken_CrudOperations_ShouldWorkCorrectly()
        {
            var user = new User { Id = Guid.NewGuid(), FullName = "Token User", Email = "token@test.com", PasswordHash = "h", Role = UserRole.Student };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = "hashed_refresh_token_123",
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };
            _context.RefreshTokens.Add(token);
            await _context.SaveChangesAsync();

            var dbToken = await _context.RefreshTokens.FindAsync(token.Id);
            Assert.NotNull(dbToken);
            Assert.False(dbToken.IsUsed);

            dbToken.IsUsed = true;
            await _context.SaveChangesAsync();

            var updatedToken = await _context.RefreshTokens.FindAsync(token.Id);
            Assert.True(updatedToken!.IsUsed);
        }
    }
}
