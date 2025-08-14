using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_BACKEND_VSA.Migrations.HRMDB
{
    /// <inheritdoc />
    public partial class updateduserdepartmentrelationship4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_email",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_User_email",
                table: "User");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email");
        }
    }
}
