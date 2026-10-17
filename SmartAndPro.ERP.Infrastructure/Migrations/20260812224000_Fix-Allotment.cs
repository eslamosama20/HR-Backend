using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAllotment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "PaymentReceiptId",
                table: "PaymentSchedule",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentSchedule_PaymentReceiptId",
                table: "PaymentSchedule",
                column: "PaymentReceiptId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentSchedule_PaymentReceipts_PaymentReceiptId",
                table: "PaymentSchedule",
                column: "PaymentReceiptId",
                principalTable: "PaymentReceipts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentSchedule_PaymentReceipts_PaymentReceiptId",
                table: "PaymentSchedule");

            migrationBuilder.DropIndex(
                name: "IX_PaymentSchedule_PaymentReceiptId",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "PaymentReceiptId",
                table: "PaymentSchedule");
        }
    }
}
