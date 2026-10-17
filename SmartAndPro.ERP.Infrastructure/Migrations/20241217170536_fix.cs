using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_InventoryStocktaking_InventoryStocktakingId",
                table: "Transaction");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_InventoryStocktaking_InventoryStocktakingId",
                table: "Transaction",
                column: "InventoryStocktakingId",
                principalTable: "InventoryStocktaking",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_InventoryStocktaking_InventoryStocktakingId",
                table: "Transaction");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_InventoryStocktaking_InventoryStocktakingId",
                table: "Transaction",
                column: "InventoryStocktakingId",
                principalTable: "InventoryStocktaking",
                principalColumn: "Id");
        }
    }
}
