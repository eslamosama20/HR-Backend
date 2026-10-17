using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorReservationsInvoices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_SaleInvoiceType",
                table: "SaleInvoice");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_IsQuotation_SaleInvoiceType",
                table: "SaleInvoice",
                columns: new[] { "IsQuotation", "SaleInvoiceType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_IsQuotation_SaleInvoiceType",
                table: "SaleInvoice");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_SaleInvoiceType",
                table: "SaleInvoice",
                column: "SaleInvoiceType");
        }
    }
}
