using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddQrCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Columns ExpiryDate, RowNumber, ShelfNumber already added in 20260127203219_AddShelfRowExpiryToPermitEntry
            // ShippingCost already added in 20260127213106_AddShippingCostToInventoryPermit
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "RowNumber",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "ShelfNumber",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "ShippingCost",
                table: "InventoryPermit");
        }
    }
}
