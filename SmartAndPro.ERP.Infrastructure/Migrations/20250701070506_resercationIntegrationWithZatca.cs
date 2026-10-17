using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class resercationIntegrationWithZatca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "ReservationCompany",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GeneratedQR",
                table: "Reservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceHash",
                table: "Reservation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSentToZatca",
                table: "Reservation",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "GeneratedQR",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "InvoiceHash",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "IsSentToZatca",
                table: "Reservation");
        }
    }
}
