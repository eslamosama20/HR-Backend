using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SplitInvoiceBaseItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_MainPurchaseClass_MainPurchaseClassId",
                table: "SaleItems");

            migrationBuilder.RenameColumn(
                name: "MainPurchaseClassId",
                table: "SaleItems",
                newName: "MainSalesClassId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_MainPurchaseClassId",
                table: "SaleItems",
                newName: "IX_SaleItems_MainSalesClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_MainSalesClass_MainSalesClassId",
                table: "SaleItems",
                column: "MainSalesClassId",
                principalTable: "MainSalesClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleItems_MainSalesClass_MainSalesClassId",
                table: "SaleItems");

            migrationBuilder.RenameColumn(
                name: "MainSalesClassId",
                table: "SaleItems",
                newName: "MainPurchaseClassId");

            migrationBuilder.RenameIndex(
                name: "IX_SaleItems_MainSalesClassId",
                table: "SaleItems",
                newName: "IX_SaleItems_MainPurchaseClassId");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleItems_MainPurchaseClass_MainPurchaseClassId",
                table: "SaleItems",
                column: "MainPurchaseClassId",
                principalTable: "MainPurchaseClass",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
