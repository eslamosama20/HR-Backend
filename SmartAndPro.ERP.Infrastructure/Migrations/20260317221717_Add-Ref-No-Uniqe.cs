using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefNoUniqe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "SaleInvoice",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "PurchaseInvoice",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_ReferenceNumber",
                table: "SaleInvoice",
                column: "ReferenceNumber",
                unique: true,
                filter: "[ReferenceNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_ReferenceNumber",
                table: "PurchaseInvoice",
                column: "ReferenceNumber",
                unique: true,
                filter: "[ReferenceNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_ReferenceNumber",
                table: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoice_ReferenceNumber",
                table: "PurchaseInvoice");

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "SaleInvoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReferenceNumber",
                table: "PurchaseInvoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
