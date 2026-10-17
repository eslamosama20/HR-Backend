using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Salla_RenameProductUnitToMainPurchaseClass : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductUnitName",
                table: "SallaProductMappings",
                newName: "MainPurchaseClassName");

            migrationBuilder.RenameColumn(
                name: "ProductUnitId",
                table: "SallaProductMappings",
                newName: "MainPurchaseClassId");

            migrationBuilder.RenameIndex(
                name: "IX_SallaProductMappings_ProductUnitId",
                table: "SallaProductMappings",
                newName: "IX_SallaProductMappings_MainPurchaseClassId");

            migrationBuilder.RenameColumn(
                name: "ProductUnitId",
                table: "SallaOrderItems",
                newName: "MainPurchaseClassId");

            migrationBuilder.RenameIndex(
                name: "IX_SallaOrderItems_ProductUnitId",
                table: "SallaOrderItems",
                newName: "IX_SallaOrderItems_MainPurchaseClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MainPurchaseClassName",
                table: "SallaProductMappings",
                newName: "ProductUnitName");

            migrationBuilder.RenameColumn(
                name: "MainPurchaseClassId",
                table: "SallaProductMappings",
                newName: "ProductUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_SallaProductMappings_MainPurchaseClassId",
                table: "SallaProductMappings",
                newName: "IX_SallaProductMappings_ProductUnitId");

            migrationBuilder.RenameColumn(
                name: "MainPurchaseClassId",
                table: "SallaOrderItems",
                newName: "ProductUnitId");

            migrationBuilder.RenameIndex(
                name: "IX_SallaOrderItems_MainPurchaseClassId",
                table: "SallaOrderItems",
                newName: "IX_SallaOrderItems_ProductUnitId");
        }
    }
}
