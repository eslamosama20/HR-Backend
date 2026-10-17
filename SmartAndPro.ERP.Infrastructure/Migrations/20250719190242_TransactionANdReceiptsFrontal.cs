using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransactionANdReceiptsFrontal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "Transaction",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "PaymentReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFrontal",
                table: "PaycheckReceipts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "PaymentReceipts");

            migrationBuilder.DropColumn(
                name: "IsFrontal",
                table: "PaycheckReceipts");
        }
    }
}
