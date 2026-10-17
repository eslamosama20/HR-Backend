using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservationEntitiesFrontal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "SalesRepresentative",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "RoomType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "ReservationView",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "ReservationSupplier",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "ReservationService",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "ReservationSeason",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "ReservationMeal",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "ReservationCompany",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "Hotel",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "SalesRepresentative");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "RoomType");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "ReservationView");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "ReservationMeal");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "Hotel");
        }
    }
}
