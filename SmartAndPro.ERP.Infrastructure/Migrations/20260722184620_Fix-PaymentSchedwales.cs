using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPaymentSchedwales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PaidAmount",
                table: "SaleInvoice",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PaymentStatus",
                table: "SaleInvoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "RemainingAmount",
                table: "SaleInvoice",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PaycheckReceiptId",
                table: "PaymentSchedule",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNo",
                table: "PaymentSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TreasuryId",
                table: "PaymentSchedule",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CreditBalance",
                table: "CustomerProfile",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_PaymentStatus",
                table: "SaleInvoice",
                column: "PaymentStatus");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_PaymentStatus",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "PaidAmount",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "PaycheckReceiptId",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "ReceiptNo",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "TreasuryId",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "CreditBalance",
                table: "CustomerProfile");
        }
    }
}
