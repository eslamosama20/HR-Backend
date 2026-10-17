using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixReservationAndWebsite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdditionalImageUrls",
                table: "PublicRoomTypes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainImagePublicId",
                table: "PublicRoomTypes",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainImageUrl",
                table: "PublicRoomTypes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PublicRoomTypes",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdditionalImageUrls",
                table: "PublicHotels",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainImagePublicId",
                table: "PublicHotels",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MainImageUrl",
                table: "PublicHotels",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "PublicHotels",
                type: "datetime2",
                nullable: true);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationActionLog");

            migrationBuilder.DropColumn(
                name: "AdditionalImageUrls",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "MainImagePublicId",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "MainImageUrl",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PublicRoomTypes");

            migrationBuilder.DropColumn(
                name: "AdditionalImageUrls",
                table: "PublicHotels");

            migrationBuilder.DropColumn(
                name: "MainImagePublicId",
                table: "PublicHotels");

            migrationBuilder.DropColumn(
                name: "MainImageUrl",
                table: "PublicHotels");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "PublicHotels");
        }
    }
}
