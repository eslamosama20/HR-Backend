using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropsForReservationSupplierAndHotel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalContactNumber",
                table: "ReservationSupplier",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BuildingNumber",
                table: "ReservationSupplier",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ReservationSupplier",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "ReservationSupplier",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "District",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Fax",
                table: "ReservationSupplier",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GaztNumber",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherId",
                table: "ReservationSupplier",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "ReservationSupplier",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortAddress",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubNumber",
                table: "ReservationSupplier",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "ReservationSupplier",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Instructions",
                table: "Hotel",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OtherId",
                table: "Hotel",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubNumber",
                table: "Hotel",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdditionalContactNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "BuildingNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "District",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Fax",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "GaztNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "OtherId",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "ShortAddress",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Street",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "SubNumber",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "Instructions",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "OtherId",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "SubNumber",
                table: "Hotel");
        }
    }
}
