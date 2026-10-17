using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptConnectionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReceiptConnection",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaycheckReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaymentReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: true),
                    PurchaseInvoiceId = table.Column<int>(type: "int", nullable: true),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LinkedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReceiptConnectionType = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptConnection", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptConnection_PaycheckReceipts_PaycheckReceiptId",
                        column: x => x.PaycheckReceiptId,
                        principalTable: "PaycheckReceipts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptConnection_PaymentReceipts_PaymentReceiptId",
                        column: x => x.PaymentReceiptId,
                        principalTable: "PaymentReceipts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptConnection_PurchaseInvoice_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptConnection_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReceiptConnection_SaleInvoice_SaleInvoiceId",
                        column: x => x.SaleInvoiceId,
                        principalTable: "SaleInvoice",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConnection_PaycheckReceiptId",
                table: "ReceiptConnection",
                column: "PaycheckReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConnection_PaymentReceiptId",
                table: "ReceiptConnection",
                column: "PaymentReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConnection_PurchaseInvoiceId",
                table: "ReceiptConnection",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConnection_ReservationId",
                table: "ReceiptConnection",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConnection_SaleInvoiceId",
                table: "ReceiptConnection",
                column: "SaleInvoiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReceiptConnection");
        }
    }
}
