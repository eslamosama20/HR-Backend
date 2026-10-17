using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentMethodDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankReferenceNumber",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckNumber",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodType",
                table: "PaymentReceipts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionNumber",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherNumber",
                table: "PaymentReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "PaycheckReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BankReferenceNumber",
                table: "PaycheckReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckNumber",
                table: "PaycheckReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransactionNumber",
                table: "PaycheckReceipts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VoucherNumber",
                table: "PaycheckReceipts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "TransactionNumber",
                table: "PaycheckReceipts");

            migrationBuilder.DropColumn(
                name: "VoucherNumber",
                table: "PaycheckReceipts");
        }
    }
}
