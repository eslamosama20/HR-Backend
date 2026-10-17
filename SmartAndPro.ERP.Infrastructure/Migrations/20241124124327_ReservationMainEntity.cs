using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservationMainEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reservation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestPhone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ArrivalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false),
                    ReservationCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationClientCompanyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesRepresentativeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HotelId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationSeasonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservation_Hotel_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotel",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservation_ReservationCompany_ReservationClientCompanyId",
                        column: x => x.ReservationClientCompanyId,
                        principalTable: "ReservationCompany",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservation_ReservationCompany_ReservationCompanyId",
                        column: x => x.ReservationCompanyId,
                        principalTable: "ReservationCompany",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservation_ReservationSeason_ReservationSeasonId",
                        column: x => x.ReservationSeasonId,
                        principalTable: "ReservationSeason",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Reservation_SalesRepresentative_SalesRepresentativeId",
                        column: x => x.SalesRepresentativeId,
                        principalTable: "SalesRepresentative",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReservationRoomEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfRooms = table.Column<int>(type: "int", nullable: false),
                    RoomRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RoomCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationMealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MealRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MealCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationViewId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViewRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ViewCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationRoomEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationRoomEntry_ReservationMeal_ReservationMealId",
                        column: x => x.ReservationMealId,
                        principalTable: "ReservationMeal",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReservationRoomEntry_ReservationView_ReservationViewId",
                        column: x => x.ReservationViewId,
                        principalTable: "ReservationView",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReservationRoomEntry_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReservationRoomEntry_RoomType_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "RoomType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReservationServiceEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CostPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReservationSupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationServiceEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReservationServiceEntry_ReservationService_ReservationServiceId",
                        column: x => x.ReservationServiceId,
                        principalTable: "ReservationService",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReservationServiceEntry_ReservationSupplier_ReservationSupplierId",
                        column: x => x.ReservationSupplierId,
                        principalTable: "ReservationSupplier",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ReservationServiceEntry_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_HotelId",
                table: "Reservation",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ReservationClientCompanyId",
                table: "Reservation",
                column: "ReservationClientCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ReservationCompanyId",
                table: "Reservation",
                column: "ReservationCompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_ReservationSeasonId",
                table: "Reservation",
                column: "ReservationSeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_SalesRepresentativeId",
                table: "Reservation",
                column: "SalesRepresentativeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationRoomEntry_ReservationId",
                table: "ReservationRoomEntry",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationRoomEntry_ReservationMealId",
                table: "ReservationRoomEntry",
                column: "ReservationMealId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationRoomEntry_ReservationViewId",
                table: "ReservationRoomEntry",
                column: "ReservationViewId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationRoomEntry_RoomTypeId",
                table: "ReservationRoomEntry",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServiceEntry_ReservationId",
                table: "ReservationServiceEntry",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServiceEntry_ReservationServiceId",
                table: "ReservationServiceEntry",
                column: "ReservationServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationServiceEntry_ReservationSupplierId",
                table: "ReservationServiceEntry",
                column: "ReservationSupplierId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationRoomEntry");

            migrationBuilder.DropTable(
                name: "ReservationServiceEntry");

            migrationBuilder.DropTable(
                name: "Reservation");
        }
    }
}
