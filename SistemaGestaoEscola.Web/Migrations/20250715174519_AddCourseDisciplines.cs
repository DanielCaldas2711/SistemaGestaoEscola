using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoEscola.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseDisciplines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CourseDisciplines",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseDisciplines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseDisciplines_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CourseDisciplines_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CourseDisciplines_CourseId",
                table: "CourseDisciplines",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseDisciplines_SubjectId",
                table: "CourseDisciplines",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseDisciplines");
        }
    }
}
