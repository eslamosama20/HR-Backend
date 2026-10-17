using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RelTransactionAndInventoryPermits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryPermitId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "InventoryPermit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_InventoryPermitId",
                table: "Transaction",
                column: "InventoryPermitId",
                unique: true,
                filter: "[InventoryPermitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryPermit_TransactionId",
                table: "InventoryPermit",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryPermit_Transaction_TransactionId",
                table: "InventoryPermit",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_InventoryPermit_InventoryPermitId",
                table: "Transaction",
                column: "InventoryPermitId",
                principalTable: "InventoryPermit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryPermit_Transaction_TransactionId",
                table: "InventoryPermit");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_InventoryPermit_InventoryPermitId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_InventoryPermitId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_InventoryPermit_TransactionId",
                table: "InventoryPermit");

            migrationBuilder.DropColumn(
                name: "InventoryPermitId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "InventoryPermit");
        }
    }
}
