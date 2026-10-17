using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierIdempotencyAndMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PaymentReference",
                table: "CashierPayment",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "CashierPayment",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StatusChangedAtUtc",
                table: "CashierPayment",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusReason",
                table: "CashierPayment",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CashierInventoryMovements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Direction = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitCost = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    InventoryPermitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CashierUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MovedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierInventoryMovements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CheckoutIdempotencyRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ResponseJson = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestedByUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckoutIdempotencyRecords", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashierInventoryMovements_InvoiceId",
                table: "CashierInventoryMovements",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInventoryMovements_MainPurchaseClassId",
                table: "CashierInventoryMovements",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInventoryMovements_MovedAtUtc",
                table: "CashierInventoryMovements",
                column: "MovedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInventoryMovements_SessionId",
                table: "CashierInventoryMovements",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierInventoryMovements_StoreDetailsId",
                table: "CashierInventoryMovements",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencyRecords_ExpiresAtUtc",
                table: "CheckoutIdempotencyRecords",
                column: "ExpiresAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CheckoutIdempotencyRecords_IdempotencyKey",
                table: "CheckoutIdempotencyRecords",
                column: "IdempotencyKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierInventoryMovements");

            migrationBuilder.DropTable(
                name: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "CashierPayment");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CashierPayment");

            migrationBuilder.DropColumn(
                name: "StatusChangedAtUtc",
                table: "CashierPayment");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "CashierPayment");
        }
    }
}
