using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductUnitsRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductBatchToUnitAssignments_ProductUnit_ProductUnitId",
                table: "ProductBatchToUnitAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_PermitEntry_PermitEntryId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_PurchaseItems_PurchaseItemId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_SupplierDetails_SupplierDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_PermitEntryId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_ProductUnitStatusId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_PurchaseItemId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_StoreDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_SupplierDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductBatchToUnitAssignments_ProductUnitId",
                table: "ProductBatchToUnitAssignments");

            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "PermitEntryId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "ProductUnitStatusId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "PurchaseItemId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "SupplierDetailsId",
                table: "ProductUnit");

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ProductUnit",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_StoreDetailsId_MainPurchaseClassId",
                table: "ProductUnit",
                columns: new[] { "StoreDetailsId", "MainPurchaseClassId" },
                unique: true,
                filter: "[StoreDetailsId] IS NOT NULL AND [MainPurchaseClassId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_StoreDetailsId_MainPurchaseClassId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProductUnit");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                table: "ProductUnit",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PermitEntryId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProductUnitStatusId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseItemId",
                table: "ProductUnit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SupplierDetailsId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_PermitEntryId",
                table: "ProductUnit",
                column: "PermitEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_ProductUnitStatusId",
                table: "ProductUnit",
                column: "ProductUnitStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_PurchaseItemId",
                table: "ProductUnit",
                column: "PurchaseItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_StoreDetailsId",
                table: "ProductUnit",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_SupplierDetailsId",
                table: "ProductUnit",
                column: "SupplierDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBatchToUnitAssignments_ProductUnitId",
                table: "ProductBatchToUnitAssignments",
                column: "ProductUnitId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBatchToUnitAssignments_ProductUnit_ProductUnitId",
                table: "ProductBatchToUnitAssignments",
                column: "ProductUnitId",
                principalTable: "ProductUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_PermitEntry_PermitEntryId",
                table: "ProductUnit",
                column: "PermitEntryId",
                principalTable: "PermitEntry",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                table: "ProductUnit",
                column: "ProductUnitStatusId",
                principalTable: "ProductUnitStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_PurchaseItems_PurchaseItemId",
                table: "ProductUnit",
                column: "PurchaseItemId",
                principalTable: "PurchaseItems",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_SupplierDetails_SupplierDetailsId",
                table: "ProductUnit",
                column: "SupplierDetailsId",
                principalTable: "SupplierDetails",
                principalColumn: "Id");
        }
    }
}
