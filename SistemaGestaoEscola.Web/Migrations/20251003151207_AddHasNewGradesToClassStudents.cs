using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoEscola.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddHasNewGradesToClassStudents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasNewGrades",
                table: "ClassStudents",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasNewGrades",
                table: "ClassStudents");
        }
    }
}
