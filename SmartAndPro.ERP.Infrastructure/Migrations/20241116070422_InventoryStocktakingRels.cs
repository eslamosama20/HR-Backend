using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryStocktakingRels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InventoryStocktakingId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualQuantity",
                table: "StocktakingEntry",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InventoryStocktakingId",
                table: "InventoryOperation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_InventoryStocktakingId",
                table: "Transaction",
                column: "InventoryStocktakingId",
                unique: true,
                filter: "[InventoryStocktakingId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOperation_InventoryStocktakingId",
                table: "InventoryOperation",
                column: "InventoryStocktakingId");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryOperation_InventoryStocktaking_InventoryStocktakingId",
                table: "InventoryOperation",
                column: "InventoryStocktakingId",
                principalTable: "InventoryStocktaking",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_InventoryStocktaking_InventoryStocktakingId",
                table: "Transaction",
                column: "InventoryStocktakingId",
                principalTable: "InventoryStocktaking",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryOperation_InventoryStocktaking_InventoryStocktakingId",
                table: "InventoryOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_InventoryStocktaking_InventoryStocktakingId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_InventoryStocktakingId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_InventoryOperation_InventoryStocktakingId",
                table: "InventoryOperation");

            migrationBuilder.DropColumn(
                name: "InventoryStocktakingId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "InventoryStocktakingId",
                table: "InventoryOperation");

            migrationBuilder.AlterColumn<decimal>(
                name: "ActualQuantity",
                table: "StocktakingEntry",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");
        }
    }
}
