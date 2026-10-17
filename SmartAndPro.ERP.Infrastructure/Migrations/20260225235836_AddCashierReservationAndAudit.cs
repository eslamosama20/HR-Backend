using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierReservationAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashierAuditLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityReference = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    PayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierAuditLog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashierStockReservation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReservedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConfirmedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleasedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReleaseReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Source = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierStockReservation", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashierAuditLog_ActionType",
                table: "CashierAuditLog",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_CashierAuditLog_CreatedAt",
                table: "CashierAuditLog",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CashierAuditLog_SessionId",
                table: "CashierAuditLog",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierAuditLog_StoreDetailsId",
                table: "CashierAuditLog",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierStockReservation_InvoiceId_MainSalesClassId",
                table: "CashierStockReservation",
                columns: new[] { "InvoiceId", "MainSalesClassId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashierStockReservation_SessionId",
                table: "CashierStockReservation",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_CashierStockReservation_StoreDetailsId_MainSalesClassId_Status",
                table: "CashierStockReservation",
                columns: new[] { "StoreDetailsId", "MainSalesClassId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierAuditLog");

            migrationBuilder.DropTable(
                name: "CashierStockReservation");
        }
    }
}
