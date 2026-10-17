using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodTypeToPaycheckReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ExecutedByUserId",
                table: "ReservationActionLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodType",
                table: "PaycheckReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationActionLog_ExecutedByUserId",
                table: "ReservationActionLog",
                column: "ExecutedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationActionLog_ReservationId",
                table: "ReservationActionLog",
                column: "ReservationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationActionLog_Users_ExecutedByUserId",
                table: "ReservationActionLog",
                column: "ExecutedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationActionLog_Users_ExecutedByUserId",
                table: "ReservationActionLog");

            migrationBuilder.DropIndex(
                name: "IX_ReservationActionLog_ExecutedByUserId",
                table: "ReservationActionLog");

            migrationBuilder.DropIndex(
                name: "IX_ReservationActionLog_ReservationId",
                table: "ReservationActionLog");

            migrationBuilder.DropColumn(
                name: "ExecutedByUserId",
                table: "ReservationActionLog");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                table: "PaycheckReceipts");
        }
    }
}
