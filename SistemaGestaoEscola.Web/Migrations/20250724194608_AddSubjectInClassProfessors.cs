using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoEscola.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddSubjectInClassProfessors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubjectId",
                table: "ClassProfessors",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ClassProfessors_SubjectId",
                table: "ClassProfessors",
                column: "SubjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClassProfessors_Subjects_SubjectId",
                table: "ClassProfessors",
                column: "SubjectId",
                principalTable: "Subjects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClassProfessors_Subjects_SubjectId",
                table: "ClassProfessors");

            migrationBuilder.DropIndex(
                name: "IX_ClassProfessors_SubjectId",
                table: "ClassProfessors");

            migrationBuilder.DropColumn(
                name: "SubjectId",
                table: "ClassProfessors");
        }
    }
}
