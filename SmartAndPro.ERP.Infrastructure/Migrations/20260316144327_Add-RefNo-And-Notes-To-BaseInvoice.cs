using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefNoAndNotesToBaseInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "SaleInvoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "SaleInvoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PurchaseQuotation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "PurchaseQuotation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "PurchaseInvoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "PurchaseInvoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "HotelAllotmentInvoice",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "HotelAllotmentInvoice",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PurchaseQuotation");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "PurchaseQuotation");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "PurchaseInvoice");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "PurchaseInvoice");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "HotelAllotmentInvoice");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "HotelAllotmentInvoice");
        }
    }
}
