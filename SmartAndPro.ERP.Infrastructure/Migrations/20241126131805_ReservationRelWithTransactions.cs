using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservationRelWithTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReservationId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TransactionId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_ReservationId",
                table: "Transaction",
                column: "ReservationId",
                unique: true,
                filter: "[ReservationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_TransactionId",
                table: "Reservation",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_Transaction_TransactionId",
                table: "Reservation",
                column: "TransactionId",
                principalTable: "Transaction",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_Reservation_ReservationId",
                table: "Transaction",
                column: "ReservationId",
                principalTable: "Reservation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_Transaction_TransactionId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_Reservation_ReservationId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_ReservationId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_TransactionId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "TransactionId",
                table: "Reservation");
        }
    }
}
