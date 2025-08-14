using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_BACKEND_VSA.Migrations.HRMDB
{
    /// <inheritdoc />
    public partial class Updatedminimunstep : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "minimunStep",
                table: "Grade",
                newName: "minimumStep");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "minimumStep",
                table: "Grade",
                newName: "minimunStep");
        }
    }
}
