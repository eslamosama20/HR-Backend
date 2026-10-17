using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductUnitRelWithPurchasseItemAndPermitEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PermitEntryId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseItemId",
                table: "ProductUnit",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_PermitEntryId",
                table: "ProductUnit",
                column: "PermitEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_PurchaseItemId",
                table: "ProductUnit",
                column: "PurchaseItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_PermitEntry_PermitEntryId",
                table: "ProductUnit",
                column: "PermitEntryId",
                principalTable: "PermitEntry",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_PurchaseItems_PurchaseItemId",
                table: "ProductUnit",
                column: "PurchaseItemId",
                principalTable: "PurchaseItems",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_PermitEntry_PermitEntryId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_PurchaseItems_PurchaseItemId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_PermitEntryId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_PurchaseItemId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "PermitEntryId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "PurchaseItemId",
                table: "ProductUnit");
        }
    }
}
