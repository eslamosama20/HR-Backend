using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTablesAndReservationSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalContactNumber",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GaztNumber",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GaztNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContactNumber",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "District",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "GaztNumber",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Hotel");

            migrationBuilder.AlterColumn<string>(
                name: "GaztNumber",
                table: "CompanyDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);
        }
    }
}
