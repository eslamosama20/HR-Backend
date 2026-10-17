using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSellingPricesToPurchaseItemAndPermitEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BulkPrice",
                table: "PurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetailPrice",
                table: "PurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WholesalePrice",
                table: "PurchaseItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            // PermitEntry columns already added in 20260331004453_AddPricesToPermitEntry
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BulkPrice",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "WholesalePrice",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "BulkPrice",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "WholesalePrice",
                table: "PermitEntry");
        }
    }
}
