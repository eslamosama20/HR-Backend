using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PdfConfigStyles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBold",
                table: "Header_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsItalic",
                table: "Header_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStrikethrough",
                table: "Header_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderline",
                table: "Header_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsBold",
                table: "Footer_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsItalic",
                table: "Footer_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStrikethrough",
                table: "Footer_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsUnderline",
                table: "Footer_Items",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBold",
                table: "Header_Items");

            migrationBuilder.DropColumn(
                name: "IsItalic",
                table: "Header_Items");

            migrationBuilder.DropColumn(
                name: "IsStrikethrough",
                table: "Header_Items");

            migrationBuilder.DropColumn(
                name: "IsUnderline",
                table: "Header_Items");

            migrationBuilder.DropColumn(
                name: "IsBold",
                table: "Footer_Items");

            migrationBuilder.DropColumn(
                name: "IsItalic",
                table: "Footer_Items");

            migrationBuilder.DropColumn(
                name: "IsStrikethrough",
                table: "Footer_Items");

            migrationBuilder.DropColumn(
                name: "IsUnderline",
                table: "Footer_Items");
        }
    }
}
