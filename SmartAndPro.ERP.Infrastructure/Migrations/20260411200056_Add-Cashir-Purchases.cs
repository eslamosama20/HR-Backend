using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashirPurchases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[MainPurchaseClass]') AND name = 'ImageUrl')
                BEGIN
                    ALTER TABLE [MainPurchaseClass] ADD [ImageUrl] nvarchar(max) NULL;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierRefundItem");

            migrationBuilder.DropTable(
                name: "CashierRefund");

            migrationBuilder.DropColumn(
                name: "BulkPrice",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "WholesalePrice",
                table: "PurchaseItems");

            migrationBuilder.DropColumn(
                name: "BulkPrice",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "RetailPrice",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "WholesalePrice",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MainPurchaseClass");
        }
    }
}
