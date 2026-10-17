using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesInvoiceModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeneratedQR",
                table: "SaleInvoice",
                type: "nvarchar(max)",
                maxLength: 2147483647,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceHash",
                table: "SaleInvoice",
                type: "nvarchar(999)",
                maxLength: 999,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCleared",
                table: "SaleInvoice",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeneratedQR",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "InvoiceHash",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "IsCleared",
                table: "SaleInvoice");
        }
    }
}
