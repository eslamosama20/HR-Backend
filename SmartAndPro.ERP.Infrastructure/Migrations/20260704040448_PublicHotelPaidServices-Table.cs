using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PublicHotelPaidServicesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PublicHotelPaidServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicHotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicPaidServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicHotelPaidServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicHotelPaidServices_PublicHotels_PublicHotelId",
                        column: x => x.PublicHotelId,
                        principalTable: "PublicHotels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicHotelPaidServices_PublicPaidServices_PublicPaidServiceId",
                        column: x => x.PublicPaidServiceId,
                        principalTable: "PublicPaidServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicHotelPaidServices_PublicHotelId",
                table: "PublicHotelPaidServices",
                column: "PublicHotelId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicHotelPaidServices_PublicPaidServiceId",
                table: "PublicHotelPaidServices",
                column: "PublicPaidServiceId");

            migrationBuilder.CreateTable(
                name: "PublicBookingPaidServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicBookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PublicHotelPaidServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PriceAtBooking = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicBookingPaidServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicBookingPaidServices_PublicBookings_PublicBookingId",
                        column: x => x.PublicBookingId,
                        principalTable: "PublicBookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PublicBookingPaidServices_PublicHotelPaidServices_PublicHotelPaidServiceId",
                        column: x => x.PublicHotelPaidServiceId,
                        principalTable: "PublicHotelPaidServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookingPaidServices_PublicBookingId",
                table: "PublicBookingPaidServices",
                column: "PublicBookingId");

            migrationBuilder.CreateIndex(
                name: "IX_PublicBookingPaidServices_PublicHotelPaidServiceId",
                table: "PublicBookingPaidServices",
                column: "PublicHotelPaidServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PublicBookingPaidServices_PublicHotelPaidServices_PublicHotelPaidServiceId",
                table: "PublicBookingPaidServices");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicHotelPaidServices_PublicHotels_PublicHotelId",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropForeignKey(
                name: "FK_PublicHotelPaidServices_PublicPaidServices_PublicPaidServiceId",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PublicHotelPaidServices",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropIndex(
                name: "IX_PublicHotelPaidServices_PublicHotelId",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropIndex(
                name: "IX_PublicHotelPaidServices_PublicPaidServiceId",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PublicHotelPaidServices");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "PublicHotelPaidServices");

            migrationBuilder.RenameTable(
                name: "PublicHotelPaidServices",
                newName: "PublicHotelPaidService");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "PublicHotelPaidService",
                newName: "TempId1");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicPaidServiceId",
                table: "PublicHotelPaidService",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<Guid>(
                name: "PublicHotelId",
                table: "PublicHotelPaidService",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PublicHotelPaidService_TempId1",
                table: "PublicHotelPaidService",
                column: "TempId1");

            migrationBuilder.AddForeignKey(
                name: "FK_PublicBookingPaidServices_PublicHotelPaidService_PublicHotelPaidServiceId",
                table: "PublicBookingPaidServices",
                column: "PublicHotelPaidServiceId",
                principalTable: "PublicHotelPaidService",
                principalColumn: "TempId1",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicHotelPaidService_PublicHotels_PublicHotelId",
                table: "PublicHotelPaidService",
                column: "PublicHotelId",
                principalTable: "PublicHotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PublicHotelPaidService_PublicPaidServices_PublicPaidServiceId",
                table: "PublicHotelPaidService",
                column: "PublicPaidServiceId",
                principalTable: "PublicPaidServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
