using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelTransactionInvoiceOneToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseInvoiceId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PurchaseInvoiceId",
                table: "Transaction",
                column: "PurchaseInvoiceId",
                unique: true,
                filter: "[PurchaseInvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SaleInvoiceId",
                table: "Transaction",
                column: "SaleInvoiceId",
                unique: true,
                filter: "[SaleInvoiceId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PurchaseInvoice_PurchaseInvoiceId",
                table: "Transaction",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_SaleInvoice_SaleInvoiceId",
                table: "Transaction",
                column: "SaleInvoiceId",
                principalTable: "SaleInvoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PurchaseInvoice_PurchaseInvoiceId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_SaleInvoice_SaleInvoiceId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PurchaseInvoiceId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SaleInvoiceId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceId",
                table: "Transaction");
        }
    }
}
