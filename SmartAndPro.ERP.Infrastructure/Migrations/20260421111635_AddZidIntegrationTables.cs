using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddZidIntegrationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_NphiesClaimId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropIndex(
                name: "IX_PharmacyInsuranceClaim_PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "CashierPaymentId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "ClaimResponseJson",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "CoveragePercentage",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsuranceCompanyId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsuranceCoveredAmount",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsuranceMemberNo",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyNo",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesAdjudicatedAt",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesClaimId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesStatus",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "NphiesSubmittedAt",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "PatientShareAmount",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "PrescriptionId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceId",
                table: "PharmacyInsuranceClaim");

            migrationBuilder.CreateTable(
                name: "ZidAuthTokens",
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
                    table.PrimaryKey("PK_ZidAuthTokens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZidOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZidOrderId = table.Column<long>(type: "bigint", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentStatus = table.Column<int>(type: "int", nullable: false),
                    FulfillmentStatus = table.Column<int>(type: "int", nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CustomerEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    ZidCustomerId = table.Column<long>(type: "bigint", nullable: true),
                    ShippingCity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingDistrict = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingStreet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingPostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShippingLat = table.Column<double>(type: "float", nullable: true),
                    ShippingLng = table.Column<double>(type: "float", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ZidPaymentId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SyncStatus = table.Column<int>(type: "int", nullable: false),
                    SyncError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ZidCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZidOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZidProductMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZidProductId = table.Column<long>(type: "bigint", nullable: false),
                    ZidSku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZidProductName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    ZidPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MainPurchaseClassName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LastSyncedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZidProductMappings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZidSyncLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestPayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResponsePayload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ZidResourceId = table.Column<long>(type: "bigint", nullable: true),
                    DurationMs = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZidSyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ZidOrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZidOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZidProductId = table.Column<long>(type: "bigint", nullable: false),
                    ZidSku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OptionsSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ZidOrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ZidOrderItems_ZidOrders_ZidOrderId",
                        column: x => x.ZidOrderId,
                        principalTable: "ZidOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrderItems_MainPurchaseClassId",
                table: "ZidOrderItems",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrderItems_ZidOrderId",
                table: "ZidOrderItems",
                column: "ZidOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrderItems_ZidProductId",
                table: "ZidOrderItems",
                column: "ZidProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrders_Status",
                table: "ZidOrders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrders_StoreDetailsId",
                table: "ZidOrders",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrders_SyncStatus",
                table: "ZidOrders",
                column: "SyncStatus");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrders_ZidCreatedAt",
                table: "ZidOrders",
                column: "ZidCreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ZidOrders_ZidOrderId",
                table: "ZidOrders",
                column: "ZidOrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZidProductMappings_MainPurchaseClassId",
                table: "ZidProductMappings",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ZidProductMappings_StoreDetailsId",
                table: "ZidProductMappings",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ZidProductMappings_ZidProductId",
                table: "ZidProductMappings",
                column: "ZidProductId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ZidSyncLogs_CreatedAt",
                table: "ZidSyncLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ZidSyncLogs_EventType",
                table: "ZidSyncLogs",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_ZidSyncLogs_Status",
                table: "ZidSyncLogs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ZidSyncLogs_ZidResourceId",
                table: "ZidSyncLogs",
                column: "ZidResourceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ZidAuthTokens");

            migrationBuilder.DropTable(
                name: "ZidOrderItems");

            migrationBuilder.DropTable(
                name: "ZidProductMappings");

            migrationBuilder.DropTable(
                name: "ZidSyncLogs");

            migrationBuilder.DropTable(
                name: "ZidOrders");

            migrationBuilder.AddColumn<Guid>(
                name: "CashierPaymentId",
                table: "PharmacyInsuranceClaim",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimResponseJson",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoveragePercentage",
                table: "PharmacyInsuranceClaim",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "InsuranceCompanyId",
                table: "PharmacyInsuranceClaim",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceCoveredAmount",
                table: "PharmacyInsuranceClaim",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InsuranceMemberNo",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InsurancePolicyNo",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NphiesAdjudicatedAt",
                table: "PharmacyInsuranceClaim",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NphiesClaimId",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NphiesStatus",
                table: "PharmacyInsuranceClaim",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NphiesSubmittedAt",
                table: "PharmacyInsuranceClaim",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PatientShareAmount",
                table: "PharmacyInsuranceClaim",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PrescriptionId",
                table: "PharmacyInsuranceClaim",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceId",
                table: "PharmacyInsuranceClaim",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim",
                column: "InsuranceCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_NphiesClaimId",
                table: "PharmacyInsuranceClaim",
                column: "NphiesClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyInsuranceClaim_PrescriptionId",
                table: "PharmacyInsuranceClaim",
                column: "PrescriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyInsuranceCompany_InsuranceCompanyId",
                table: "PharmacyInsuranceClaim",
                column: "InsuranceCompanyId",
                principalTable: "PharmacyInsuranceCompany",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PharmacyInsuranceClaim_PharmacyPrescription_PrescriptionId",
                table: "PharmacyInsuranceClaim",
                column: "PrescriptionId",
                principalTable: "PharmacyPrescription",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
