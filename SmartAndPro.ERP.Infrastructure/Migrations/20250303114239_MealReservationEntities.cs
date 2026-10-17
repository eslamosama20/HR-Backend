using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MealReservationEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTransactionPost1",
                table: "Transaction",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "MealReservationId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MealReservationId",
                table: "Attachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MealReservation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealReservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealReservation_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MealReservation_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "MealReservationEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArrivalDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DepartureDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NumberOfNights = table.Column<int>(type: "int", nullable: false),
                    ReservationMealId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NumberOfPersons = table.Column<int>(type: "int", nullable: false),
                    Frees = table.Column<int>(type: "int", nullable: false),
                    MealRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MealCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EntringMeal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExitingMeal = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MealReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MealReservationEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MealReservationEntry_MealReservation_MealReservationId",
                        column: x => x.MealReservationId,
                        principalTable: "MealReservation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MealReservationEntry_ReservationMeal_ReservationMealId",
                        column: x => x.ReservationMealId,
                        principalTable: "ReservationMeal",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_MealReservationId",
                table: "Transaction",
                column: "MealReservationId",
                unique: true,
                filter: "[MealReservationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_MealReservationId",
                table: "Attachments",
                column: "MealReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_MealReservation_CustomerProfileId",
                table: "MealReservation",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_MealReservation_TransactionId",
                table: "MealReservation",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MealReservationEntry_MealReservationId",
                table: "MealReservationEntry",
                column: "MealReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_MealReservationEntry_ReservationMealId",
                table: "MealReservationEntry",
                column: "ReservationMealId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_MealReservation_MealReservationId",
                table: "Attachments",
                column: "MealReservationId",
                principalTable: "MealReservation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_MealReservation_MealReservationId",
                table: "Transaction",
                column: "MealReservationId",
                principalTable: "MealReservation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_MealReservation_MealReservationId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_MealReservation_MealReservationId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "MealReservationEntry");

            migrationBuilder.DropTable(
                name: "MealReservation");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_MealReservationId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_MealReservationId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "IsTransactionPost1",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "MealReservationId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "MealReservationId",
                table: "Attachments");
        }
    }
}
