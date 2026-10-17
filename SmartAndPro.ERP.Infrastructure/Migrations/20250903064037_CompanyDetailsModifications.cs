using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CompanyDetailsModifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ProfilePicture",
                table: "Users",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PageOrientation",
                table: "ReservationPrintTableTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "BankAccountId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyPhoneId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyPolicyId",
                table: "Reservation",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Reservation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_BankAccountId",
                table: "Reservation",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CompanyPhoneId",
                table: "Reservation",
                column: "CompanyPhoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_CompanyPolicyId",
                table: "Reservation",
                column: "CompanyPolicyId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_BankAccount_BankAccountId",
                table: "Reservation",
                column: "BankAccountId",
                principalTable: "BankAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_CompanyPhones_CompanyPhoneId",
                table: "Reservation",
                column: "CompanyPhoneId",
                principalTable: "CompanyPhones",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_CompanyPolicy_CompanyPolicyId",
                table: "Reservation",
                column: "CompanyPolicyId",
                principalTable: "CompanyPolicy",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_BankAccount_BankAccountId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_CompanyPhones_CompanyPhoneId",
                table: "Reservation");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_CompanyPolicy_CompanyPolicyId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_BankAccountId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_CompanyPhoneId",
                table: "Reservation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_CompanyPolicyId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "ProfilePicture",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PageOrientation",
                table: "ReservationPrintTableTemplates");

            migrationBuilder.DropColumn(
                name: "BankAccountId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "CompanyPhoneId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "CompanyPolicyId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Reservation");
        }
    }
}
