using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHRModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDocuments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ApplicantName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApplicantPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ApplicantEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicantIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Position = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ApplicationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Experience = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Education = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpectedSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmploymentType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WeeklyDayOff = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmploymentType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FingerprintDeviceSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Factory1DeviceIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory1DevicePort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory1DeviceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Factory2DeviceIP = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory2DevicePort = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory2DeviceEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Factory1LastSyncTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Factory1LastSyncStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory1LastSyncMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory1LastSyncFetchedCount = table.Column<int>(type: "int", nullable: false),
                    Factory1LastSyncInsertedCount = table.Column<int>(type: "int", nullable: false),
                    Factory1LastSyncSkippedCount = table.Column<int>(type: "int", nullable: false),
                    Factory2LastSyncTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Factory2LastSyncStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory2LastSyncMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Factory2LastSyncFetchedCount = table.Column<int>(type: "int", nullable: false),
                    Factory2LastSyncInsertedCount = table.Column<int>(type: "int", nullable: false),
                    Factory2LastSyncSkippedCount = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FingerprintDeviceSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shift",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    GracePeriodInMinutes = table.Column<int>(type: "int", maxLength: 300, nullable: false, defaultValue: 0),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shift", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmploymentTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MonthlyInsurance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    LeaveBalance = table.Column<int>(type: "int", nullable: false),
                    MonthlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplementaryRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FactoryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Employee_EmploymentType_EmploymentTypeId",
                        column: x => x.EmploymentTypeId,
                        principalTable: "EmploymentType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Employee_Shift_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Attendance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AttendanceFlag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attendance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attendance_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankDocuments_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bonuses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bonuses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bonuses_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CheckInLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Time = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LogType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckInLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheckInLog_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HRTrustReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemSerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ItemValue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ReceiveDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReturnDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsReturned = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HRTrustReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HRTrustReceipts_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "InsuranceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InsuranceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InsuranceType = table.Column<int>(type: "int", nullable: false),
                    InsuranceProvider = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: false),
                    MonthlyPremium = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    CoverageAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InsuranceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InsuranceRecords_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "LeaveApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeaveType = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalDays = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApproverId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveApplications_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MonthlyLoans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlyLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MonthlyLoans_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OvertimeHours",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    HoursCount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OvertimeHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OvertimeHours_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Penalties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Penalties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Penalties_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResignationRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestDate = table.Column<DateOnly>(type: "date", nullable: false),
                    LastWorkingDay = table.Column<DateOnly>(type: "date", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResignationRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResignationRequests_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SalarySlips",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Allowances = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Overtime = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Bonus = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Deductions = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Insurance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    NetSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PaymentDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPaid = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExpirationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PreReminderDays = table.Column<int>(type: "int", nullable: false),
                    IsCritical = table.Column<bool>(type: "bit", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalarySlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalarySlips_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employee",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attendance_EmployeeId_AttendanceDate",
                table: "Attendance",
                columns: new[] { "EmployeeId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankDocuments_EmployeeId",
                table: "BankDocuments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_BankDocuments_ExpirationDate",
                table: "BankDocuments",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_BankDocuments_ReferenceNumber",
                table: "BankDocuments",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bonuses_EmployeeId",
                table: "Bonuses",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckInLog_EmployeeId_Time",
                table: "CheckInLog",
                columns: new[] { "EmployeeId", "Time" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDocuments_ExpirationDate",
                table: "CompanyDocuments",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDocuments_IsCritical",
                table: "CompanyDocuments",
                column: "IsCritical");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDocuments_ReferenceNumber",
                table: "CompanyDocuments",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_EmploymentTypeId",
                table: "Employee",
                column: "EmploymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_Name",
                table: "Employee",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ReferenceNumber",
                table: "Employee",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employee_ShiftId",
                table: "Employee",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentApplications_ExpirationDate",
                table: "EmploymentApplications",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentApplications_ReferenceNumber",
                table: "EmploymentApplications",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentApplications_Status",
                table: "EmploymentApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_EmploymentType_Code",
                table: "EmploymentType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HRTrustReceipts_EmployeeId",
                table: "HRTrustReceipts",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HRTrustReceipts_ExpirationDate",
                table: "HRTrustReceipts",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_HRTrustReceipts_IsReturned",
                table: "HRTrustReceipts",
                column: "IsReturned");

            migrationBuilder.CreateIndex(
                name: "IX_HRTrustReceipts_ReferenceNumber",
                table: "HRTrustReceipts",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_EmployeeId",
                table: "InsuranceRecords",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_ExpirationDate",
                table: "InsuranceRecords",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_InsuranceType",
                table: "InsuranceRecords",
                column: "InsuranceType");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_IsCritical",
                table: "InsuranceRecords",
                column: "IsCritical");

            migrationBuilder.CreateIndex(
                name: "IX_InsuranceRecords_ReferenceNumber",
                table: "InsuranceRecords",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_EmployeeId_StartDate",
                table: "LeaveApplications",
                columns: new[] { "EmployeeId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_ExpirationDate",
                table: "LeaveApplications",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_LeaveType",
                table: "LeaveApplications",
                column: "LeaveType");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_ReferenceNumber",
                table: "LeaveApplications",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaveApplications_Status",
                table: "LeaveApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlyLoans_EmployeeId",
                table: "MonthlyLoans",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OvertimeHours_EmployeeId",
                table: "OvertimeHours",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Penalties_EmployeeId",
                table: "Penalties",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_EmployeeId",
                table: "ResignationRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_ExpirationDate",
                table: "ResignationRequests",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_ReferenceNumber",
                table: "ResignationRequests",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResignationRequests_Status",
                table: "ResignationRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_EmployeeId_Year_Month",
                table: "SalarySlips",
                columns: new[] { "EmployeeId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_ExpirationDate",
                table: "SalarySlips",
                column: "ExpirationDate");

            migrationBuilder.CreateIndex(
                name: "IX_SalarySlips_ReferenceNumber",
                table: "SalarySlips",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shift_Name",
                table: "Shift",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attendance");

            migrationBuilder.DropTable(
                name: "BankDocuments");

            migrationBuilder.DropTable(
                name: "Bonuses");

            migrationBuilder.DropTable(
                name: "CheckInLog");

            migrationBuilder.DropTable(
                name: "CompanyDocuments");

            migrationBuilder.DropTable(
                name: "EmploymentApplications");

            migrationBuilder.DropTable(
                name: "FingerprintDeviceSettings");

            migrationBuilder.DropTable(
                name: "HRTrustReceipts");

            migrationBuilder.DropTable(
                name: "InsuranceRecords");

            migrationBuilder.DropTable(
                name: "LeaveApplications");

            migrationBuilder.DropTable(
                name: "MonthlyLoans");

            migrationBuilder.DropTable(
                name: "OvertimeHours");

            migrationBuilder.DropTable(
                name: "Penalties");

            migrationBuilder.DropTable(
                name: "ResignationRequests");

            migrationBuilder.DropTable(
                name: "SalarySlips");

            migrationBuilder.DropTable(
                name: "Employee");

            migrationBuilder.DropTable(
                name: "EmploymentType");

            migrationBuilder.DropTable(
                name: "Shift");
        }
    }
}
