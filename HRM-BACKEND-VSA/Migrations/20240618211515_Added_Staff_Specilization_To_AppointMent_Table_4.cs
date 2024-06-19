using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_BACKEND_VSA.Migrations
{
    /// <inheritdoc />
    public partial class Added_Staff_Specilization_To_AppointMent_Table_4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applicant",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    firsName = table.Column<string>(type: "text", nullable: false),
                    lastName = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    contact = table.Column<string>(type: "text", nullable: false),
                    hasSubmittedApplication = table.Column<bool>(type: "boolean", nullable: true),
                    applicationStatus = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applicant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantBioData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicantId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    surName = table.Column<string>(type: "text", nullable: false),
                    firstName = table.Column<string>(type: "text", nullable: false),
                    otherNames = table.Column<string>(type: "text", nullable: true),
                    gender = table.Column<string>(type: "text", nullable: false),
                    citizenship = table.Column<string>(type: "text", nullable: false),
                    dateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    SNNITNumber = table.Column<string>(type: "text", nullable: true),
                    phoneOne = table.Column<string>(type: "text", nullable: false),
                    phoneTwo = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    GPSAddress = table.Column<string>(type: "text", nullable: true),
                    disability = table.Column<string>(type: "text", nullable: true),
                    ECOWASCardNumber = table.Column<string>(type: "text", nullable: false),
                    passportNumber = table.Column<string>(type: "text", nullable: true),
                    passportPicture = table.Column<string>(type: "text", nullable: true),
                    birthCertificate = table.Column<string>(type: "text", nullable: true),
                    highestQualification = table.Column<string>(type: "text", nullable: true),
                    highestQualificationCertificate = table.Column<string>(type: "text", nullable: true),
                    nssNumber = table.Column<string>(type: "text", nullable: true),
                    yearOfService = table.Column<DateOnly>(type: "date", nullable: false),
                    placeOfService = table.Column<string>(type: "text", nullable: true),
                    nssCertificate = table.Column<string>(type: "text", nullable: true),
                    controllerStaffNumber = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantBioData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicantBioData_Applicant_applicantId",
                        column: x => x.applicantId,
                        principalTable: "Applicant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantHasOTP",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    contact = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    otp = table.Column<string>(type: "text", nullable: false),
                    applicantID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantHasOTP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicantHasOTP_Applicant_applicantID",
                        column: x => x.applicantID,
                        principalTable: "Applicant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicantEducationalBackground",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    applicantBioDataId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    yearCompleted = table.Column<DateOnly>(type: "date", nullable: false),
                    institutionName = table.Column<string>(type: "text", nullable: false),
                    certificate = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicantEducationalBackground", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicantEducationalBackground_ApplicantBioData_applicantBi~",
                        column: x => x.applicantBioDataId,
                        principalTable: "ApplicantBioData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantBioData_applicantId",
                table: "ApplicantBioData",
                column: "applicantId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantBioData_phoneOne",
                table: "ApplicantBioData",
                column: "phoneOne",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantEducationalBackground_applicantBioDataId",
                table: "ApplicantEducationalBackground",
                column: "applicantBioDataId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicantHasOTP_applicantID",
                table: "ApplicantHasOTP",
                column: "applicantID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicantEducationalBackground");

            migrationBuilder.DropTable(
                name: "ApplicantHasOTP");

            migrationBuilder.DropTable(
                name: "ApplicantBioData");

            migrationBuilder.DropTable(
                name: "Applicant");
        }
    }
}
