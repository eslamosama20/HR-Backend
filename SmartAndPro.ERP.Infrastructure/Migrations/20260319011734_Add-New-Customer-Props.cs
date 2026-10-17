using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewCustomerProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalContactNumber",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GaztNumber",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "CustomerProfile",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContactNumber",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "District",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "GaztNumber",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "CustomerProfile");
        }
    }
}
