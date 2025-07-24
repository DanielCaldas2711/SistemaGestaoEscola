using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestaoEscola.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddClassProfessors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassProfessors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassId = table.Column<int>(type: "int", nullable: false),
                    ProfessorId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassProfessors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassProfessors_AspNetUsers_ProfessorId",
                        column: x => x.ProfessorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClassProfessors_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassProfessors_ClassId",
                table: "ClassProfessors",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassProfessors_ProfessorId",
                table: "ClassProfessors",
                column: "ProfessorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassProfessors");
        }
    }
}
