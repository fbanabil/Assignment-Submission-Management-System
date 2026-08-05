using AssignmentSystem.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data
{
    public static class EntityConfigurations
    {
        public static void Configure(this ModelBuilder modelBuilder)
        {
            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(e => e.Role)
                    .HasConversion<string>();

                entity.HasMany(e => e.CreatedAssignments)
                    .WithOne(a => a.Teacher)
                    .HasForeignKey(a => a.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.StudentSubmissions)
                    .WithOne(s => s.Student)
                    .HasForeignKey(s => s.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.GradedSubmissions)
                    .WithOne(s => s.GradeGiver)
                    .HasForeignKey(s => s.GradedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.TeacherAssignments)
                    .WithOne(ta => ta.Teacher)
                    .HasForeignKey(ta => ta.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.StudentEnrollments)
                    .WithOne(se => se.Student)
                    .HasForeignKey(se => se.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Class configuration
            modelBuilder.Entity<Class>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Section)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.AcademicYear)
                    .IsRequired()
                    .HasMaxLength(50);


                entity.HasMany(e => e.ClassSubjects)
                    .WithOne(cs => cs.Class)
                    .HasForeignKey(cs => cs.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.StudentEnrollments)
                    .WithOne(se => se.Class)
                    .HasForeignKey(se => se.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Assignments)
                    .WithOne(a => a.Class)
                    .HasForeignKey(a => a.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Subject configuration
            modelBuilder.Entity<Subject>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Code)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Code).IsUnique();

                entity.HasMany(e => e.ClassSubjects)
                    .WithOne(cs => cs.Subject)
                    .HasForeignKey(cs => cs.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Assignments)
                    .WithOne(a => a.Subject)
                    .HasForeignKey(a => a.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ClassSubject configuration
            modelBuilder.Entity<ClassSubject>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Class)
                    .WithMany(c => c.ClassSubjects)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.ClassSubjects)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.TeacherAssignments)
                    .WithOne(ta => ta.ClassSubject)
                    .HasForeignKey(ta => ta.ClassSubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // TeacherAssignment configuration
            modelBuilder.Entity<TeacherAssignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Teacher)
                    .WithMany(u => u.TeacherAssignments)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ClassSubject)
                    .WithMany(cs => cs.TeacherAssignments)
                    .HasForeignKey(e => e.ClassSubjectId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // StudentEnrollment configuration
            modelBuilder.Entity<StudentEnrollment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Student)
                    .WithMany(u => u.StudentEnrollments)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Class)
                    .WithMany(c => c.StudentEnrollments)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            // Assignment configuration
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(2000);

                entity.Property(e => e.Status)
                    .HasConversion<string>();

                entity.Property(e => e.Deadline)
                    .IsRequired();

                entity.Property(e => e.MaxMarks)
                    .IsRequired();



                entity.HasOne(e => e.Class)
                    .WithMany(c => c.Assignments)
                    .HasForeignKey(e => e.ClassId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Subject)
                    .WithMany(s => s.Assignments)
                    .HasForeignKey(e => e.SubjectId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Teacher)
                    .WithMany(u => u.CreatedAssignments)
                    .HasForeignKey(e => e.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.Submissions)
                    .WithOne(s => s.Assignment)
                    .HasForeignKey(s => s.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Submission configuration
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.SubmissionText)
                    .HasMaxLength(4000);

                entity.Property(e => e.FileUrl)
                    .HasMaxLength(500);

                entity.Property(e => e.Feedback)
                    .HasMaxLength(1000);

                entity.Property(e => e.Status)
                    .HasConversion<string>();


                entity.HasOne(e => e.Assignment)
                    .WithMany(a => a.Submissions)
                    .HasForeignKey(e => e.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Student)
                    .WithMany(u => u.StudentSubmissions)
                    .HasForeignKey(e => e.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.GradeGiver)
                    .WithMany(u => u.GradedSubmissions)
                    .HasForeignKey(e => e.GradedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

        }
    }
}
