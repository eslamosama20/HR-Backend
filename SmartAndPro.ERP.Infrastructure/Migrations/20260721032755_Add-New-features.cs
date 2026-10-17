using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNewfeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationActionLog_Users_ExecutedByUserId",
                table: "ReservationActionLog");

            migrationBuilder.DropTable(
                name: "CustomerDepartments");

            migrationBuilder.DropIndex(
                name: "IX_ReservationActionLog_ExecutedByUserId",
                table: "ReservationActionLog");

            migrationBuilder.DropIndex(
                name: "IX_ReservationActionLog_ReservationId",
                table: "ReservationActionLog");

            migrationBuilder.DropIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "ExecutedByUserId",
                table: "ReservationActionLog");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "BankReferenceNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "CheckNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "TransactionNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "VoucherNumber",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "BankReferenceNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "CheckNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "TransactionNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "VoucherNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "IsForProjects",
                table: "CustomerProfile");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts",
                column: "ReferenceNumber",
                unique: true);
        }
    }
}
