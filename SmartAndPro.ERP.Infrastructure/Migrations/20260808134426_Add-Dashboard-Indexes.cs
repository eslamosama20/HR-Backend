using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_IssueDate",
                table: "SaleInvoice",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ArrivalDate",
                table: "Reservation",
                column: "ArrivalDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_IsCancelled",
                table: "Reservation",
                column: "IsCancelled");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_IsConfirmed",
                table: "Reservation",
                column: "IsConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_IssueDate",
                table: "PurchaseInvoice",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReceiptDate",
                table: "PaymentReceipts",
                column: "ReceiptDate");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_ReceiptDate",
                table: "PaycheckReceipts",
                column: "ReceiptDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_IssueDate",
                table: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_ArrivalDate",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_IsCancelled",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_IsConfirmed",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoice_IssueDate",
                table: "PurchaseInvoice");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_ReceiptDate",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaycheckReceipts_ReceiptDate",
                table: "PaycheckReceipts");
        }
    }
}
