using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReservationRelWithMainAccounts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "SalesRepresentative",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "ReservationSupplier",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseMainAccountId",
                table: "ReservationService",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevenueMainAccountId",
                table: "ReservationService",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExpenseMainAccountId",
                table: "ReservationSeason",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RevenueMainAccountId",
                table: "ReservationSeason",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "ReservationCompany",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "Hotel",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesRepresentative_MainAccountId",
                table: "SalesRepresentative",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSupplier_MainAccountId",
                table: "ReservationSupplier",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationService_ExpenseMainAccountId",
                table: "ReservationService",
                column: "ExpenseMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationService_RevenueMainAccountId",
                table: "ReservationService",
                column: "RevenueMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_ExpenseMainAccountId",
                table: "ReservationSeason",
                column: "ExpenseMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationSeason_RevenueMainAccountId",
                table: "ReservationSeason",
                column: "RevenueMainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservationCompany_MainAccountId",
                table: "ReservationCompany",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Hotel_MainAccountId",
                table: "Hotel",
                column: "MainAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Hotel_MainAccount_MainAccountId",
                table: "Hotel",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationCompany_MainAccount_MainAccountId",
                table: "ReservationCompany",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationService_MainAccount_ExpenseMainAccountId",
                table: "ReservationService",
                column: "ExpenseMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationService_MainAccount_RevenueMainAccountId",
                table: "ReservationService",
                column: "RevenueMainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationSupplier_MainAccount_MainAccountId",
                table: "ReservationSupplier",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SalesRepresentative_MainAccount_MainAccountId",
                table: "SalesRepresentative",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Hotel_MainAccount_MainAccountId",
                table: "Hotel");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationCompany_MainAccount_MainAccountId",
                table: "ReservationCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_ExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSeason_MainAccount_RevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationService_MainAccount_ExpenseMainAccountId",
                table: "ReservationService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationService_MainAccount_RevenueMainAccountId",
                table: "ReservationService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReservationSupplier_MainAccount_MainAccountId",
                table: "ReservationSupplier");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesRepresentative_MainAccount_MainAccountId",
                table: "SalesRepresentative");

            migrationBuilder.DropIndex(
                name: "IX_SalesRepresentative_MainAccountId",
                table: "SalesRepresentative");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSupplier_MainAccountId",
                table: "ReservationSupplier");

            migrationBuilder.DropIndex(
                name: "IX_ReservationService_ExpenseMainAccountId",
                table: "ReservationService");

            migrationBuilder.DropIndex(
                name: "IX_ReservationService_RevenueMainAccountId",
                table: "ReservationService");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_ExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationSeason_RevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropIndex(
                name: "IX_ReservationCompany_MainAccountId",
                table: "ReservationCompany");

            migrationBuilder.DropIndex(
                name: "IX_Hotel_MainAccountId",
                table: "Hotel");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "SalesRepresentative");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "ReservationSupplier");

            migrationBuilder.DropColumn(
                name: "ExpenseMainAccountId",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "RevenueMainAccountId",
                table: "ReservationService");

            migrationBuilder.DropColumn(
                name: "ExpenseMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "RevenueMainAccountId",
                table: "ReservationSeason");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "ReservationCompany");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "Hotel");
        }
    }
}
