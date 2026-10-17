using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAllotmentUsageTableAndRefactorAllotmentInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HotelAllotmentInvoiceId",
                table: "Reservation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AllotmentUsage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelAllotmentRoomId = table.Column<int>(type: "int", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UsageDate = table.Column<DateTime>(type: "date", nullable: false),
                    ConsumedRooms = table.Column<int>(type: "int", nullable: false),
                    PricePerRoom = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OverbookedRooms = table.Column<int>(type: "int", nullable: false),
                    MealPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ViewId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllotmentUsage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AllotmentUsage_HotelAllotmentRoom_HotelAllotmentRoomId",
                        column: x => x.HotelAllotmentRoomId,
                        principalTable: "HotelAllotmentRoom",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AllotmentUsage_ReservationMeal_MealPlanId",
                        column: x => x.MealPlanId,
                        principalTable: "ReservationMeal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AllotmentUsage_ReservationView_ViewId",
                        column: x => x.ViewId,
                        principalTable: "ReservationView",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AllotmentUsage_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HotelAllotmentInvoiceId",
                table: "Reservation",
                column: "HotelAllotmentInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_AllotmentUsage_HotelAllotmentRoomId_UsageDate",
                table: "AllotmentUsage",
                columns: new[] { "HotelAllotmentRoomId", "UsageDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AllotmentUsage_MealPlanId",
                table: "AllotmentUsage",
                column: "MealPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_AllotmentUsage_ReservationId",
                table: "AllotmentUsage",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_AllotmentUsage_ViewId",
                table: "AllotmentUsage",
                column: "ViewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "Reservation",
                column: "HotelAllotmentInvoiceId",
                principalTable: "HotelAllotmentInvoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_HotelAllotmentInvoice_HotelAllotmentInvoiceId",
                table: "Reservation");

            migrationBuilder.DropTable(
                name: "AllotmentUsage");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_HotelAllotmentInvoiceId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "HotelAllotmentInvoiceId",
                table: "Reservation");
        }
    }
}
