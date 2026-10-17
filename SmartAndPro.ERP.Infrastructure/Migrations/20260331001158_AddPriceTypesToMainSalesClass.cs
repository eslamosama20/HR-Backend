using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceTypesToMainSalesClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BulkPrice",
                table: "MainSalesClass",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "RetailPrice",
                table: "MainSalesClass",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WholesalePrice",
                table: "MainSalesClass",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BulkPrice",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "WholesalePrice",
                table: "MainSalesClass");
        }
    }
}
