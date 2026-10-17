using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyDispenseAndInventoryEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "StoreCategory",
                table: "StoreDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "general",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "General");

            migrationBuilder.AlterColumn<string>(
                name: "SfdaCode",
                table: "MainPurchaseClass",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "NeedsExpiry",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<bool>(
                name: "NeedsBatch",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "ItemType",
                table: "MainPurchaseClass",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "general",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldDefaultValue: "General");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPharmacyItem",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateTable(
                name: "PharmacyAllocation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocationReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CommitMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ConfirmedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PreparedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyAllocation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyAllocation_PharmacyPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "PharmacyPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyAllocationLine",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MainPurchaseClassId = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RequestedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommittedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AvailableQuantityAtCommit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyAllocationLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyAllocationLine_PharmacyAllocation_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "PharmacyAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PharmacyPickup",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AllocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AllocationReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AllocationCommitMode = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    RxNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PatientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PaymentType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    InsuranceStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    PrescriptionStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PosTerminalId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CashierUserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PickupStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InsuranceCovered = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PatientPayable = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PosInvoiceId = table.Column<int>(type: "int", nullable: true),
                    PosInvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PharmacyPickup", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PharmacyPickup_PharmacyAllocation_AllocationId",
                        column: x => x.AllocationId,
                        principalTable: "PharmacyAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PharmacyPickup_PharmacyPrescription_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "PharmacyPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAllocation_AllocationReferenceId",
                table: "PharmacyAllocation",
                column: "AllocationReferenceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAllocation_PrescriptionId",
                table: "PharmacyAllocation",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyAllocationLine_AllocationId",
                table: "PharmacyAllocationLine",
                column: "AllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPickup_AllocationId",
                table: "PharmacyPickup",
                column: "AllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPickup_PickupStatus",
                table: "PharmacyPickup",
                column: "PickupStatus");

            migrationBuilder.CreateIndex(
                name: "IX_PharmacyPickup_PrescriptionId",
                table: "PharmacyPickup",
                column: "PrescriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmacyAllocationLine");

            migrationBuilder.DropTable(
                name: "PharmacyPickup");

            migrationBuilder.DropTable(
                name: "PharmacyAllocation");

            migrationBuilder.AlterColumn<string>(
                name: "StoreCategory",
                table: "StoreDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "General",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "general");

            migrationBuilder.AlterColumn<string>(
                name: "SfdaCode",
                table: "MainPurchaseClass",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "NeedsExpiry",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "NeedsBatch",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "ItemType",
                table: "MainPurchaseClass",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "General",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "general");

            migrationBuilder.AlterColumn<bool>(
                name: "IsPharmacyItem",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);
        }
    }
}
