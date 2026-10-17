using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPropsForSupplierTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalContactNumber",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GaztNumber",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "SupplierDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContactNumber",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "District",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "GaztNumber",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "SupplierDetails");
        }
    }
}
