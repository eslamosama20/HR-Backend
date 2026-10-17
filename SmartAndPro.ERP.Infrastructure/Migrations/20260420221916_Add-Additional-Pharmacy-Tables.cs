using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdditionalPharmacyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PharmacyAllocationLine");

            migrationBuilder.DropTable(
                name: "PharmacyPickup");

            migrationBuilder.DropTable(
                name: "PharmacyAllocation");

            migrationBuilder.DropColumn(
                name: "StoreCategory",
                table: "StoreDetails");

            migrationBuilder.DropColumn(
                name: "BatchNo",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "IsPharmacyItem",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "ItemType",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "NeedsBatch",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "NeedsExpiry",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "SfdaCode",
                table: "MainPurchaseClass");
        }
    }
}
