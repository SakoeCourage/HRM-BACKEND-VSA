using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_BACKEND_VSA.Migrations.HRMDB
{
    /// <inheritdoc />
    public partial class addedStaffSepeartion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "postingOption",
                table: "StaffPosting",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "staffIdentificationNumber",
                table: "Staff",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "Seperation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StaffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    DateOfSeparation = table.Column<DateOnly>(type: "date", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seperation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seperation_Staff_StaffId",
                        column: x => x.StaffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Seperation_StaffId",
                table: "Seperation",
                column: "StaffId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Seperation");

            migrationBuilder.DropColumn(
                name: "postingOption",
                table: "StaffPosting");

            migrationBuilder.AlterColumn<string>(
                name: "staffIdentificationNumber",
                table: "Staff",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
