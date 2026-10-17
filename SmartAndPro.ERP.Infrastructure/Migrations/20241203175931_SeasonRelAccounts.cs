using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeasonRelAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_ExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_RevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.RenameColumn(
                name: "RevenueMainAccountId",
                table: "ReservationSeason",
                newName: "RoomRevenueMainAccountId");

            migrationBuilder.RenameColumn(
                name: "ExpenseMainAccountId",
                table: "ReservationSeason",
                newName: "RoomExpenseMainAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationSeason_RevenueMainAccountId",
                table: "ReservationSeason",
                newName: "IX_ReservationSeason_RoomRevenueMainAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationSeason_ExpenseMainAccountId",
                table: "ReservationSeason",
                newName: "IX_ReservationSeason_RoomExpenseMainAccountId");

            migrationBuilder.AddColumn<Guid>(
                name: "MealExpenseMainAccountId",
                table: "ReservationSeason",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MealRevenueMainAccountId",
                table: "ReservationSeason",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OtherExpenseMainAccountId",
                table: "ReservationSeason",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OtherRevenueMainAccountId",
                table: "ReservationSeason",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationCompanyId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_MealExpenseMainAccountId",
                table: "ReservationSeason",
                column: "MealExpenseMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_MealRevenueMainAccountId",
                table: "ReservationSeason",
                column: "MealRevenueMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_OtherExpenseMainAccountId",
                table: "ReservationSeason",
                column: "OtherExpenseMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_OtherRevenueMainAccountId",
                table: "ReservationSeason",
                column: "OtherRevenueMainAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_MealExpenseMainAccountId",
                table: "ReservationSeason",
                column: "MealExpenseMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_MealRevenueMainAccountId",
                table: "ReservationSeason",
                column: "MealRevenueMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_OtherExpenseMainAccountId",
                table: "ReservationSeason",
                column: "OtherExpenseMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_OtherRevenueMainAccountId",
                table: "ReservationSeason",
                column: "OtherRevenueMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_RoomExpenseMainAccountId",
                table: "ReservationSeason",
                column: "RoomExpenseMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_RoomRevenueMainAccountId",
                table: "ReservationSeason",
                column: "RoomRevenueMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_MealExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_MealRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_OtherExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_OtherRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_RoomExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_RoomRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_MealExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_MealRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_OtherExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_OtherRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "MealExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "MealRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "OtherExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "OtherRevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.RenameColumn(
                name: "RoomRevenueMainAccountId",
                table: "ReservationSeason",
                newName: "RevenueMainAccountId");

            migrationBuilder.RenameColumn(
                name: "RoomExpenseMainAccountId",
                table: "ReservationSeason",
                newName: "ExpenseMainAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationSeason_RoomRevenueMainAccountId",
                table: "ReservationSeason",
                newName: "IX_ReservationSeason_RevenueMainAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationSeason_RoomExpenseMainAccountId",
                table: "ReservationSeason",
                newName: "IX_ReservationSeason_ExpenseMainAccountId");

            migrationBuilder.AlterColumn<Guid>(
                name: "ReservationCompanyId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_ExpenseMainAccountId",
                table: "ReservationSeason",
                column: "ExpenseMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSeason_MainAccount_RevenueMainAccountId",
                table: "ReservationSeason",
                column: "RevenueMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }
    }
}
