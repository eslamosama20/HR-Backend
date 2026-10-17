using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeReservationReferenceNumberUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
              name: "UX_Reservation_ReferenceNumber",
              table: "Reservation",
              column: "ReferenceNumber",
              unique: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
          name: "UX_Reservation_ReferenceNumber",
          table: "Reservation");
        }
    }
}
