using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReceiptsRelWithTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaycheckReceiptId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentReceiptId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "PaymentReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "PaycheckReceipts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PaycheckReceiptId",
                table: "Transaction",
                column: "PaycheckReceiptId",
                unique: true,
                filter: "[PaycheckReceiptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PaymentReceiptId",
                table: "Transaction",
                column: "PaymentReceiptId",
                unique: true,
                filter: "[PaymentReceiptId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_TransactionId",
                table: "PaymentReceipts",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_TransactionId",
                table: "PaycheckReceipts",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

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
                name: "FK_Transaction_PaycheckReceipts_PaycheckReceiptId",
                table: "Transaction",
                column: "PaycheckReceiptId",
                principalTable: "PaycheckReceipts",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PaymentReceipts_PaymentReceiptId",
                table: "Transaction",
                column: "PaymentReceiptId",
                principalTable: "PaymentReceipts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaycheckReceipts_Transaction_TransactionId",
                table: "PaycheckReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_PaymentReceipts_Transaction_TransactionId",
                table: "PaymentReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PaycheckReceipts_PaycheckReceiptId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PaymentReceipts_PaymentReceiptId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PaycheckReceiptId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PaymentReceiptId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_TransactionId",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaycheckReceipts_TransactionId",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "PaycheckReceiptId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PaymentReceiptId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "PaycheckReceipts");
        }
    }
}
