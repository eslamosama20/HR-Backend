using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestrictedDrugDispense");

            migrationBuilder.DropColumn(
                name: "BatchNo",
                table: "PermitEntry");

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Provider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PaymentUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrderReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WebhookPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ExternalId",
                table: "Payments",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Method",
                table: "Payments",
                column: "Method");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.AddColumn<string>(
                name: "BatchNo",
                table: "PermitEntry",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RestrictedDrugDispense",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AuthorizationNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DispenseReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DispensedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DispensedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DoctorName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PharmacistName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RestrictionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestrictedDrugDispense", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RestrictedDrugDispense_ItemCode",
                table: "RestrictedDrugDispense",
                column: "ItemCode");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictedDrugDispense_Status",
                table: "RestrictedDrugDispense",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RestrictedDrugDispense_StoreDetailsId",
                table: "RestrictedDrugDispense",
                column: "StoreDetailsId");
        }
    }
}
