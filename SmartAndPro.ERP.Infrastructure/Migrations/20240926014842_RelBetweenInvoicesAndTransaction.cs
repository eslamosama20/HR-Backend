using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelBetweenInvoicesAndTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "PurchaseInvoice",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_TransactionId",
                table: "SaleInvoice",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_TransactionId",
                table: "PurchaseInvoice",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoice_Transaction_TransactionId",
                table: "PurchaseInvoice",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_Transaction_TransactionId",
                table: "SaleInvoice",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoice_Transaction_TransactionId",
                table: "PurchaseInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_Transaction_TransactionId",
                table: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_TransactionId",
                table: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoice_TransactionId",
                table: "PurchaseInvoice");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "PurchaseInvoice");
        }
    }
}
