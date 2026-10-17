using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWasafatyIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WasafatyAuthTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessToken = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WasafatyAuthTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WasafatyPrescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FhirResourceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PatientNationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DoctorLicenseNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrescriberFacilityName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    InsuranceName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InsurancePolicyNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InsuranceCoveragePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    ItemsCount = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ClaimStatus = table.Column<int>(type: "int", nullable: false),
                    NphiesClaimId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    InternalPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CashierPaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OtpVerified = table.Column<bool>(type: "bit", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RawFhirJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WasafatyPrescriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WasafatySyncLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FhirResourceId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WasafatySyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WasafatyPrescriptionItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WasafatyPrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DrugCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DrugName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DosageForm = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Strength = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DosageInstruction = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: true),
                    RequestedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DispensedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsuranceCoveredAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PatientPayAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AllowSubstitute = table.Column<bool>(type: "bit", nullable: false),
                    IsControlled = table.Column<bool>(type: "bit", nullable: false),
                    ErpProductId = table.Column<int>(type: "int", nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WasafatyPrescriptionItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WasafatyPrescriptionItems_WasafatyPrescriptions_WasafatyPrescriptionId",
                        column: x => x.WasafatyPrescriptionId,
                        principalTable: "WasafatyPrescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptionItems_DrugCode",
                table: "WasafatyPrescriptionItems",
                column: "DrugCode");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptionItems_WasafatyPrescriptionId",
                table: "WasafatyPrescriptionItems",
                column: "WasafatyPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptions_CashierPaymentId",
                table: "WasafatyPrescriptions",
                column: "CashierPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptions_FhirResourceId",
                table: "WasafatyPrescriptions",
                column: "FhirResourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptions_InternalPrescriptionId",
                table: "WasafatyPrescriptions",
                column: "InternalPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptions_PatientNationalId",
                table: "WasafatyPrescriptions",
                column: "PatientNationalId");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptions_ReceivedAt",
                table: "WasafatyPrescriptions",
                column: "ReceivedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatyPrescriptions_Status",
                table: "WasafatyPrescriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatySyncLogs_CreatedAt",
                table: "WasafatySyncLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatySyncLogs_EventType",
                table: "WasafatySyncLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_WasafatySyncLogs_Status",
                table: "WasafatySyncLogs",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WasafatyAuthTokens");

            migrationBuilder.DropTable(
                name: "WasafatyPrescriptionItems");

            migrationBuilder.DropTable(
                name: "WasafatySyncLogs");

            migrationBuilder.DropTable(
                name: "WasafatyPrescriptions");
        }
    }
}
