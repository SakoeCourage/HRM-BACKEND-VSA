using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRM_BACKEND_VSA.Migrations.HRMDB
{
    /// <inheritdoc />
    public partial class UpdatedUserEntitySchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allowance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    allowance = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allowance", x => x.Id);
                });

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
                name: "Bank",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    bankName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bank", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    categoryName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    data = table.Column<string>(type: "text", nullable: false),
                    notifiableType = table.Column<string>(type: "text", nullable: false),
                    notifiableId = table.Column<Guid>(type: "uuid", nullable: false),
                    readAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permission",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permission", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalBody",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalBody", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SMSTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    readOnly = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaxRate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    year = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRate", x => x.Id);
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
                name: "Grade",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    categoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    gradeName = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<string>(type: "text", nullable: false),
                    scale = table.Column<string>(type: "text", nullable: false),
                    marketPremium = table.Column<double>(type: "double precision", nullable: false),
                    minimunStep = table.Column<int>(type: "integer", nullable: false),
                    maximumStep = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Grade_Category_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Speciality",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    categoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    specialityName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Speciality", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Speciality_Category_categoryId",
                        column: x => x.categoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleHasPermissions",
                columns: table => new
                {
                    roleId = table.Column<Guid>(type: "uuid", nullable: false),
                    permissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleHasPermissions", x => new { x.roleId, x.permissionId });
                    table.ForeignKey(
                        name: "FK_RoleHasPermissions_Permission_permissionId",
                        column: x => x.permissionId,
                        principalTable: "Permission",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleHasPermissions_Role_roleId",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SMSCampaignHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    campaignName = table.Column<string>(type: "text", nullable: false),
                    smsTemplateId = table.Column<Guid>(type: "uuid", nullable: true),
                    message = table.Column<string>(type: "text", nullable: false),
                    receipients = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSCampaignHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SMSCampaignHistory_SMSTemplate_smsTemplateId",
                        column: x => x.smsTemplateId,
                        principalTable: "SMSTemplate",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TaxRateDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    taxRateId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    taxableIncome = table.Column<double>(type: "double precision", nullable: false),
                    rate = table.Column<double>(type: "double precision", nullable: false),
                    taxCode = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaxRateDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaxRateDetail_TaxRate_taxRateId",
                        column: x => x.taxRateId,
                        principalTable: "TaxRate",
                        principalColumn: "Id");
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

            migrationBuilder.CreateTable(
                name: "GradeStep",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    gradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    stepIndex = table.Column<int>(type: "integer", nullable: false),
                    salary = table.Column<double>(type: "double precision", nullable: false),
                    marketPreBaseSalary = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GradeStep", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GradeStep_Grade_gradeId",
                        column: x => x.gradeId,
                        principalTable: "Grade",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    GPSAddress = table.Column<string>(type: "text", nullable: false),
                    staffIdentificationNumber = table.Column<string>(type: "text", nullable: false),
                    firstName = table.Column<string>(type: "text", nullable: false),
                    lastName = table.Column<string>(type: "text", nullable: false),
                    otherNames = table.Column<string>(type: "text", nullable: true),
                    specialityId = table.Column<Guid>(type: "uuid", nullable: true),
                    dateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    SNNITNumber = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    disability = table.Column<string>(type: "text", nullable: false),
                    passportPicture = table.Column<string>(type: "text", nullable: true),
                    ECOWASCardNumber = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_Speciality_specialityId",
                        column: x => x.specialityId,
                        principalTable: "Speciality",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SMSCampaignReceipient",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    campaignHistoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    contact = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    firstName = table.Column<string>(type: "text", nullable: true),
                    lastName = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "text", nullable: true, comment: "Status => Pending or Successful or Failed")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSCampaignReceipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SMSCampaignReceipient_SMSCampaignHistory_campaignHistoryId",
                        column: x => x.campaignHistoryId,
                        principalTable: "SMSCampaignHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Directorate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    directorateName = table.Column<string>(type: "text", nullable: false),
                    directorId = table.Column<Guid>(type: "uuid", nullable: true),
                    depDirectoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Directorate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Directorate_Staff_depDirectoryId",
                        column: x => x.depDirectoryId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Directorate_Staff_directorId",
                        column: x => x.directorId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffAccomodationDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    gpsAddress = table.Column<string>(type: "text", nullable: false),
                    accomodationType = table.Column<string>(type: "text", nullable: false),
                    allocationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    flatNumber = table.Column<string>(type: "text", nullable: true),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAccomodationDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAccomodationDetail_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffAccomodationUpdateHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    gpsAddress = table.Column<string>(type: "text", nullable: false),
                    accomodationType = table.Column<string>(type: "text", nullable: false),
                    allocationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    flatNumber = table.Column<string>(type: "text", nullable: true),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAccomodationUpdateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAccomodationUpdateHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffAppointment",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    gradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    staffSpecialityId = table.Column<Guid>(type: "uuid", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    appointmentType = table.Column<string>(type: "text", nullable: false),
                    staffType = table.Column<string>(type: "text", nullable: false),
                    endDate = table.Column<DateOnly>(type: "date", nullable: true),
                    paymentSource = table.Column<string>(type: "text", nullable: false),
                    notionalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    substantiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    step = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAppointment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAppointment_Grade_gradeId",
                        column: x => x.gradeId,
                        principalTable: "Grade",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAppointment_Speciality_staffSpecialityId",
                        column: x => x.staffSpecialityId,
                        principalTable: "Speciality",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAppointment_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffAppointmentHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    gradeId = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    appointmentType = table.Column<string>(type: "text", nullable: false),
                    staffSpecialityId = table.Column<Guid>(type: "uuid", nullable: true),
                    endDate = table.Column<DateOnly>(type: "date", nullable: true),
                    staffType = table.Column<string>(type: "text", nullable: false),
                    paymentSource = table.Column<string>(type: "text", nullable: false),
                    notionalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    substantiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    step = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffAppointmentHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffAppointmentHistory_Grade_gradeId",
                        column: x => x.gradeId,
                        principalTable: "Grade",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAppointmentHistory_Speciality_staffSpecialityId",
                        column: x => x.staffSpecialityId,
                        principalTable: "Speciality",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffAppointmentHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffBankDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bankId = table.Column<Guid>(type: "uuid", nullable: false),
                    accountType = table.Column<string>(type: "text", nullable: false),
                    branch = table.Column<string>(type: "text", nullable: false),
                    accountNumber = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffBankDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffBankDetail_Bank_bankId",
                        column: x => x.bankId,
                        principalTable: "Bank",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffBankDetail_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffBankUpdateHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    bankId = table.Column<Guid>(type: "uuid", nullable: false),
                    accountType = table.Column<string>(type: "text", nullable: false),
                    branch = table.Column<string>(type: "text", nullable: false),
                    accountNumber = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffBankUpdateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffBankUpdateHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffBioUpdateHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    GPSAddress = table.Column<string>(type: "text", nullable: false),
                    staffIdentificationNumber = table.Column<string>(type: "text", nullable: false),
                    firstName = table.Column<string>(type: "text", nullable: false),
                    lastName = table.Column<string>(type: "text", nullable: false),
                    otherNames = table.Column<string>(type: "text", nullable: true),
                    ECOWASCardNumber = table.Column<string>(type: "text", nullable: false),
                    specialityId = table.Column<Guid>(type: "uuid", nullable: true),
                    dateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    SNNITNumber = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    disability = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffBioUpdateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffBioUpdateHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffChildrenDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    childName = table.Column<string>(type: "text", nullable: false),
                    dateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffChildrenDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffChildrenDetail_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffChildrenUpdateHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    childName = table.Column<string>(type: "text", nullable: false),
                    dateOfBirth = table.Column<DateOnly>(type: "date", nullable: false),
                    gender = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffChildrenUpdateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffChildrenUpdateHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffFamilyDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fathersName = table.Column<string>(type: "text", nullable: false),
                    mothersName = table.Column<string>(type: "text", nullable: false),
                    spouseName = table.Column<string>(type: "text", nullable: false),
                    spousePhoneNumber = table.Column<string>(type: "text", nullable: false),
                    nextOfKIN = table.Column<string>(type: "text", nullable: false),
                    nextOfKINPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    emergencyPerson = table.Column<string>(type: "text", nullable: false),
                    emergencyPersonPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffFamilyDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffFamilyDetail_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffFamilyUpdatetHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fathersName = table.Column<string>(type: "text", nullable: false),
                    mothersName = table.Column<string>(type: "text", nullable: false),
                    spouseName = table.Column<string>(type: "text", nullable: false),
                    spousePhoneNumber = table.Column<string>(type: "text", nullable: false),
                    nextOfKIN = table.Column<string>(type: "text", nullable: false),
                    nextOfKINPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    emergencyPerson = table.Column<string>(type: "text", nullable: false),
                    emergencyPersonPhoneNumber = table.Column<string>(type: "text", nullable: false),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffFamilyUpdatetHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffFamilyUpdatetHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffProfessionalLincense",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    professionalBodyId = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    pin = table.Column<string>(type: "text", nullable: false),
                    issuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    expiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffProfessionalLincense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffProfessionalLincense_ProfessionalBody_professionalBody~",
                        column: x => x.professionalBodyId,
                        principalTable: "ProfessionalBody",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffProfessionalLincense_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffProfessionalLincenseUpdateHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    professionalBodyId = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    pin = table.Column<string>(type: "text", nullable: false),
                    issuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    expiryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isApproved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffProfessionalLincenseUpdateHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffProfessionalLincenseUpdateHistory_ProfessionalBody_pro~",
                        column: x => x.professionalBodyId,
                        principalTable: "ProfessionalBody",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StaffProfessionalLincenseUpdateHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StaffRequest",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    requestFromStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    requestAssignedStaffId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestDetailPolymorphicId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    requestType = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffRequest_Staff_requestAssignedStaffId",
                        column: x => x.requestAssignedStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffRequest_Staff_requestFromStaffId",
                        column: x => x.requestFromStaffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Department",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    directorateId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    departmentName = table.Column<string>(type: "text", nullable: false),
                    headOfDepartmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    depHeadOfDepartmentId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Department_Directorate_directorateId",
                        column: x => x.directorateId,
                        principalTable: "Directorate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Department_Staff_depHeadOfDepartmentId",
                        column: x => x.depHeadOfDepartmentId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Department_Staff_headOfDepartmentId",
                        column: x => x.headOfDepartmentId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Unit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    departmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    unitHeadId = table.Column<Guid>(type: "uuid", nullable: true),
                    directorateId = table.Column<Guid>(type: "uuid", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    unitName = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Unit_Department_departmentId",
                        column: x => x.departmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Unit_Directorate_directorateId",
                        column: x => x.directorateId,
                        principalTable: "Directorate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Unit_Staff_unitHeadId",
                        column: x => x.unitHeadId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffPosting",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    directorateId = table.Column<Guid>(type: "uuid", nullable: true),
                    departmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    unitId = table.Column<Guid>(type: "uuid", nullable: true),
                    postingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    isAlterable = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPosting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffPosting_Department_departmentId",
                        column: x => x.departmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffPosting_Directorate_directorateId",
                        column: x => x.directorateId,
                        principalTable: "Directorate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffPosting_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffPosting_Unit_unitId",
                        column: x => x.unitId,
                        principalTable: "Unit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StaffPostingHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    directorateId = table.Column<Guid>(type: "uuid", nullable: true),
                    departmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    unitId = table.Column<Guid>(type: "uuid", nullable: true),
                    postingDate = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StaffPostingHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StaffPostingHistory_Department_departmentId",
                        column: x => x.departmentId,
                        principalTable: "Department",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffPostingHistory_Directorate_directorateId",
                        column: x => x.directorateId,
                        principalTable: "Directorate",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffPostingHistory_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StaffPostingHistory_Unit_unitId",
                        column: x => x.unitId,
                        principalTable: "Unit",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    emailVerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    lastSeen = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    isAccountActive = table.Column<bool>(type: "boolean", nullable: false),
                    hasResetPassword = table.Column<bool>(type: "boolean", nullable: false),
                    staffId = table.Column<Guid>(type: "uuid", nullable: false),
                    roleId = table.Column<Guid>(type: "uuid", nullable: false),
                    unitId = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    departmentId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_User_Role_roleId",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_User_Staff_staffId",
                        column: x => x.staffId,
                        principalTable: "Staff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_User_Unit_unitId",
                        column: x => x.unitId,
                        principalTable: "Unit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserHasOTP",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    createdAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    otp = table.Column<string>(type: "text", nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHasOTP", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserHasOTP_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserHasRole",
                columns: table => new
                {
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    roleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserHasRole", x => new { x.userId, x.roleId });
                    table.ForeignKey(
                        name: "FK_UserHasRole_Role_roleId",
                        column: x => x.roleId,
                        principalTable: "Role",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserHasRole_User_userId",
                        column: x => x.userId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Allowance_code",
                table: "Allowance",
                column: "code",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_Category_categoryName",
                table: "Category",
                column: "categoryName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_depHeadOfDepartmentId",
                table: "Department",
                column: "depHeadOfDepartmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Department_directorateId",
                table: "Department",
                column: "directorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Department_headOfDepartmentId",
                table: "Department",
                column: "headOfDepartmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Directorate_depDirectoryId",
                table: "Directorate",
                column: "depDirectoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Directorate_directorId",
                table: "Directorate",
                column: "directorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Grade_categoryId",
                table: "Grade",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Grade_gradeName",
                table: "Grade",
                column: "gradeName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GradeStep_gradeId",
                table: "GradeStep",
                column: "gradeId");

            migrationBuilder.CreateIndex(
                name: "IX_Permission_name",
                table: "Permission",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Role_name",
                table: "Role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleHasPermissions_permissionId",
                table: "RoleHasPermissions",
                column: "permissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SMSCampaignHistory_smsTemplateId",
                table: "SMSCampaignHistory",
                column: "smsTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SMSCampaignReceipient_campaignHistoryId",
                table: "SMSCampaignReceipient",
                column: "campaignHistoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Speciality_categoryId",
                table: "Speciality",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Speciality_specialityName",
                table: "Speciality",
                column: "specialityName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_ECOWASCardNumber",
                table: "Staff",
                column: "ECOWASCardNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_email",
                table: "Staff",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_phone",
                table: "Staff",
                column: "phone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_SNNITNumber",
                table: "Staff",
                column: "SNNITNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Staff_specialityId",
                table: "Staff",
                column: "specialityId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_staffIdentificationNumber",
                table: "Staff",
                column: "staffIdentificationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAccomodationDetail_staffId",
                table: "StaffAccomodationDetail",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAccomodationUpdateHistory_staffId",
                table: "StaffAccomodationUpdateHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointment_gradeId",
                table: "StaffAppointment",
                column: "gradeId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointment_staffId",
                table: "StaffAppointment",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointment_staffSpecialityId",
                table: "StaffAppointment",
                column: "staffSpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointmentHistory_gradeId",
                table: "StaffAppointmentHistory",
                column: "gradeId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointmentHistory_staffId",
                table: "StaffAppointmentHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffAppointmentHistory_staffSpecialityId",
                table: "StaffAppointmentHistory",
                column: "staffSpecialityId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBankDetail_bankId",
                table: "StaffBankDetail",
                column: "bankId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBankDetail_staffId",
                table: "StaffBankDetail",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffBankUpdateHistory_staffId",
                table: "StaffBankUpdateHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffBioUpdateHistory_staffId",
                table: "StaffBioUpdateHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffChildrenDetail_staffId",
                table: "StaffChildrenDetail",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffChildrenUpdateHistory_staffId",
                table: "StaffChildrenUpdateHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffFamilyDetail_staffId",
                table: "StaffFamilyDetail",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffFamilyUpdatetHistory_staffId",
                table: "StaffFamilyUpdatetHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_departmentId",
                table: "StaffPosting",
                column: "departmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_directorateId",
                table: "StaffPosting",
                column: "directorateId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_staffId",
                table: "StaffPosting",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPosting_unitId",
                table: "StaffPosting",
                column: "unitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffPostingHistory_departmentId",
                table: "StaffPostingHistory",
                column: "departmentId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPostingHistory_directorateId",
                table: "StaffPostingHistory",
                column: "directorateId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPostingHistory_staffId",
                table: "StaffPostingHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffPostingHistory_unitId",
                table: "StaffPostingHistory",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfessionalLincense_professionalBodyId",
                table: "StaffProfessionalLincense",
                column: "professionalBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfessionalLincense_staffId",
                table: "StaffProfessionalLincense",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfessionalLincenseUpdateHistory_professionalBodyId",
                table: "StaffProfessionalLincenseUpdateHistory",
                column: "professionalBodyId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffProfessionalLincenseUpdateHistory_staffId",
                table: "StaffProfessionalLincenseUpdateHistory",
                column: "staffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRequest_requestAssignedStaffId",
                table: "StaffRequest",
                column: "requestAssignedStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRequest_RequestDetailPolymorphicId",
                table: "StaffRequest",
                column: "RequestDetailPolymorphicId");

            migrationBuilder.CreateIndex(
                name: "IX_StaffRequest_requestFromStaffId",
                table: "StaffRequest",
                column: "requestFromStaffId");

            migrationBuilder.CreateIndex(
                name: "IX_TaxRate_year",
                table: "TaxRate",
                column: "year",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaxRateDetail_taxRateId",
                table: "TaxRateDetail",
                column: "taxRateId");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_departmentId",
                table: "Unit",
                column: "departmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_directorateId",
                table: "Unit",
                column: "directorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Unit_unitHeadId",
                table: "Unit",
                column: "unitHeadId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_departmentId",
                table: "User",
                column: "departmentId");

            migrationBuilder.CreateIndex(
                name: "IX_User_email",
                table: "User",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_roleId",
                table: "User",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "IX_User_staffId",
                table: "User",
                column: "staffId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_unitId",
                table: "User",
                column: "unitId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHasOTP_userId",
                table: "UserHasOTP",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_UserHasRole_roleId",
                table: "UserHasRole",
                column: "roleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Allowance");

            migrationBuilder.DropTable(
                name: "ApplicantEducationalBackground");

            migrationBuilder.DropTable(
                name: "ApplicantHasOTP");

            migrationBuilder.DropTable(
                name: "GradeStep");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "RoleHasPermissions");

            migrationBuilder.DropTable(
                name: "SMSCampaignReceipient");

            migrationBuilder.DropTable(
                name: "StaffAccomodationDetail");

            migrationBuilder.DropTable(
                name: "StaffAccomodationUpdateHistory");

            migrationBuilder.DropTable(
                name: "StaffAppointment");

            migrationBuilder.DropTable(
                name: "StaffAppointmentHistory");

            migrationBuilder.DropTable(
                name: "StaffBankDetail");

            migrationBuilder.DropTable(
                name: "StaffBankUpdateHistory");

            migrationBuilder.DropTable(
                name: "StaffBioUpdateHistory");

            migrationBuilder.DropTable(
                name: "StaffChildrenDetail");

            migrationBuilder.DropTable(
                name: "StaffChildrenUpdateHistory");

            migrationBuilder.DropTable(
                name: "StaffFamilyDetail");

            migrationBuilder.DropTable(
                name: "StaffFamilyUpdatetHistory");

            migrationBuilder.DropTable(
                name: "StaffPosting");

            migrationBuilder.DropTable(
                name: "StaffPostingHistory");

            migrationBuilder.DropTable(
                name: "StaffProfessionalLincense");

            migrationBuilder.DropTable(
                name: "StaffProfessionalLincenseUpdateHistory");

            migrationBuilder.DropTable(
                name: "StaffRequest");

            migrationBuilder.DropTable(
                name: "TaxRateDetail");

            migrationBuilder.DropTable(
                name: "UserHasOTP");

            migrationBuilder.DropTable(
                name: "UserHasRole");

            migrationBuilder.DropTable(
                name: "ApplicantBioData");

            migrationBuilder.DropTable(
                name: "Permission");

            migrationBuilder.DropTable(
                name: "SMSCampaignHistory");

            migrationBuilder.DropTable(
                name: "Grade");

            migrationBuilder.DropTable(
                name: "Bank");

            migrationBuilder.DropTable(
                name: "ProfessionalBody");

            migrationBuilder.DropTable(
                name: "TaxRate");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Applicant");

            migrationBuilder.DropTable(
                name: "SMSTemplate");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.DropTable(
                name: "Unit");

            migrationBuilder.DropTable(
                name: "Department");

            migrationBuilder.DropTable(
                name: "Directorate");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Speciality");

            migrationBuilder.DropTable(
                name: "Category");
        }
    }
}
