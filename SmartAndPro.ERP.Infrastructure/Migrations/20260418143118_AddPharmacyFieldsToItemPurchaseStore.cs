using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPharmacyFieldsToItemPurchaseStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreCategory",
                table: "StoreDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "General");

            migrationBuilder.AddColumn<string>(
                name: "BatchNo",
                table: "PurchaseItems",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpiryDate",
                table: "PurchaseItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPharmacyItem",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ItemType",
                table: "MainPurchaseClass",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "General");

            migrationBuilder.AddColumn<bool>(
                name: "NeedsBatch",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NeedsExpiry",
                table: "MainPurchaseClass",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "SfdaCode",
                table: "MainPurchaseClass",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
