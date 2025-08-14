using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_BACKEND_VSA.Migrations.HRMDB
{
    /// <inheritdoc />
    public partial class updateduserdepartmentrelationship5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Drop the foreign key constraint
            // migrationBuilder.DropForeignKey(
            //     name: "FK_User_Department_DepartmentId", // name of your FK
            //     table: "User");

            // 2. Drop the index for the foreign key
            // migrationBuilder.DropIndex(
            //     name: "IX_User_DepartmentId",
            //     table: "User");

            // 3. Drop the column
            // migrationBuilder.DropColumn(
            //     name: "DepartmentId",
            //     table: "User");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Add the column back
            // migrationBuilder.AddColumn<int>(
            //     name: "DepartmentId",
            //     table: "User",
            //     type: "integer",
            //     nullable: false,
            //     defaultValue: 0);

            // Recreate the index
            // migrationBuilder.CreateIndex(
            //     name: "IX_User_DepartmentId",
            //     table: "User",
            //     column: "DepartmentId");

            // Recreate the foreign key
            // migrationBuilder.AddForeignKey(
            //     name: "FK_User_Department_DepartmentId",
            //     table: "User",
            //     column: "DepartmentId",
            //     principalTable: "Department",
            //     principalColumn: "Id",
            //     onDelete: ReferentialAction.Cascade);
        }
    }
}

