using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddRollNoToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherAssignments_TeacherId",
                table: "TeacherAssignments");

            migrationBuilder.DropIndex(
                name: "IX_StudentEnrollments_StudentId",
                table: "StudentEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_ClassSubjects_ClassId",
                table: "ClassSubjects");

            migrationBuilder.AddColumn<string>(
                name: "RollNo",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TeacherId_ClassSubjectId",
                table: "TeacherAssignments",
                columns: new[] { "TeacherId", "ClassSubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_StudentId_ClassId",
                table: "StudentEnrollments",
                columns: new[] { "StudentId", "ClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId",
                table: "ClassSubjects",
                columns: new[] { "ClassId", "SubjectId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeacherAssignments_TeacherId_ClassSubjectId",
                table: "TeacherAssignments");

            migrationBuilder.DropIndex(
                name: "IX_StudentEnrollments_StudentId_ClassId",
                table: "StudentEnrollments");

            migrationBuilder.DropIndex(
                name: "IX_ClassSubjects_ClassId_SubjectId",
                table: "ClassSubjects");

            migrationBuilder.DropColumn(
                name: "RollNo",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherAssignments_TeacherId",
                table: "TeacherAssignments",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_StudentId",
                table: "StudentEnrollments",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassId",
                table: "ClassSubjects",
                column: "ClassId");
        }
    }
}
