using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTaxExempted",
                table: "PurchaseItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItemArabicName",
                table: "PurchaseItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ItemEnglishName",
                table: "PurchaseItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTaxExempted",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "ItemArabicName",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "ItemEnglishName",
                table: "PurchaseItems");
        }
    }
}
