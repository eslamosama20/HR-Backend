using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixStocktakingRels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryOperation_InventoryStocktaking_InventoryStocktakingId",
                table: "InventoryOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryOperation_Transaction_TransactionId",
                table: "InventoryOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocktaking_Transaction_TransactionId",
                table: "InventoryStocktaking");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryOperation_InventoryStocktaking_InventoryStocktakingId",
                table: "InventoryOperation",
                column: "InventoryStocktakingId",
                principalTable: "InventoryStocktaking",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryOperation_Transaction_TransactionId",
                table: "InventoryOperation",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocktaking_Transaction_TransactionId",
                table: "InventoryStocktaking",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InventoryOperation_InventoryStocktaking_InventoryStocktakingId",
                table: "InventoryOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryOperation_Transaction_TransactionId",
                table: "InventoryOperation");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryStocktaking_Transaction_TransactionId",
                table: "InventoryStocktaking");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryOperation_InventoryStocktaking_InventoryStocktakingId",
                table: "InventoryOperation",
                column: "InventoryStocktakingId",
                principalTable: "InventoryStocktaking",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryOperation_Transaction_TransactionId",
                table: "InventoryOperation",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryStocktaking_Transaction_TransactionId",
                table: "InventoryStocktaking",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");
        }
    }
}
