using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OnDeleteSetNullTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryPermit_Transaction_TransactionId",
                table: "InventoryPermit");

            migrationBuilder.DropForeignKey(
                name: "FK_PaycheckReceipts_Transaction_TransactionId",
                table: "PaycheckReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Transaction_TransactionId",
                table: "PaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoice_Transaction_TransactionId",
                table: "PurchaseInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturn_Transaction_TransactionId",
                table: "PurchaseReturn");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Transaction_TransactionId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_Transaction_TransactionId",
                table: "SaleInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturn_Transaction_TransactionId",
                table: "SalesReturn");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryPermit_Transaction_TransactionId",
                table: "InventoryPermit",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaycheckReceipts_Transaction_TransactionId",
                table: "PaycheckReceipts",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Transaction_TransactionId",
                table: "PaymentReceipts",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoice_Transaction_TransactionId",
                table: "PurchaseInvoice",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturn_Transaction_TransactionId",
                table: "PurchaseReturn",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Transaction_TransactionId",
                table: "Reservation",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_Transaction_TransactionId",
                table: "SaleInvoice",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturn_Transaction_TransactionId",
                table: "SalesReturn",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryPermit_Transaction_TransactionId",
                table: "InventoryPermit");

            migrationBuilder.DropForeignKey(
                name: "FK_PaycheckReceipts_Transaction_TransactionId",
                table: "PaycheckReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Transaction_TransactionId",
                table: "PaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoice_Transaction_TransactionId",
                table: "PurchaseInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturn_Transaction_TransactionId",
                table: "PurchaseReturn");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Transaction_TransactionId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_Transaction_TransactionId",
                table: "SaleInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturn_Transaction_TransactionId",
                table: "SalesReturn");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryPermit_Transaction_TransactionId",
                table: "InventoryPermit",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaycheckReceipts_Transaction_TransactionId",
                table: "PaycheckReceipts",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentReceipts_Transaction_TransactionId",
                table: "PaymentReceipts",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoice_Transaction_TransactionId",
                table: "PurchaseInvoice",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturn_Transaction_TransactionId",
                table: "PurchaseReturn",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Transaction_TransactionId",
                table: "Reservation",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_Transaction_TransactionId",
                table: "SaleInvoice",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturn_Transaction_TransactionId",
                table: "SalesReturn",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");
        }
    }
}
