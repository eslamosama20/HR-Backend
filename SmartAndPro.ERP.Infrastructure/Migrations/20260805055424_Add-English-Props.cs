using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnglishProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnglishCity",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishCountry",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishDistrict",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishStreet",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EnglishDistrict",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnglishCity",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "EnglishCountry",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "EnglishDistrict",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "EnglishStreet",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "EnglishDistrict",
                table: "CompanyDetails");
        }
    }
}
