using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedProductDetailsAndRefactoredProductUnitToBeChildToMainClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MainPurchaseClass_StoreDetails_StoreDetailsId",
                table: "MainPurchaseClass");

            migrationBuilder.DropForeignKey(
                name: "FK_MainSalesClass_StoreDetails_StoreDetailsId",
                table: "MainSalesClass");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_ProductDetails_ProductDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                table: "ProductUnit");

            migrationBuilder.DropTable(
                name: "ProductDetails");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_ProductDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_StoreDetailsId",
                table: "MainSalesClass");

            migrationBuilder.DropIndex(
                name: "IX_MainPurchaseClass_StoreDetailsId",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "ProductDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "StoreDetailsId",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "StoreDetailsId",
                table: "MainPurchaseClass");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductUnitStatusId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "ExpiryDate",
                table: "ProductUnit",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<Guid>(
                name: "MainPurchaseClassId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MainSalesClassId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreDetailsId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_MainPurchaseClassId",
                table: "ProductUnit",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_MainSalesClassId",
                table: "ProductUnit",
                column: "MainSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_StoreDetailsId",
                table: "ProductUnit",
                column: "StoreDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_MainPurchaseClass_MainPurchaseClassId",
                table: "ProductUnit",
                column: "MainPurchaseClassId",
                principalTable: "MainPurchaseClass",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_MainSalesClass_MainSalesClassId",
                table: "ProductUnit",
                column: "MainSalesClassId",
                principalTable: "MainSalesClass",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                table: "ProductUnit",
                column: "ProductUnitStatusId",
                principalTable: "ProductUnitStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_StoreDetails_StoreDetailsId",
                table: "ProductUnit",
                column: "StoreDetailsId",
                principalTable: "StoreDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_MainPurchaseClass_MainPurchaseClassId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_MainSalesClass_MainSalesClassId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                table: "ProductUnit");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnit_StoreDetails_StoreDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_MainPurchaseClassId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_MainSalesClassId",
                table: "ProductUnit");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnit_StoreDetailsId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "MainPurchaseClassId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "MainSalesClassId",
                table: "ProductUnit");

            migrationBuilder.DropColumn(
                name: "StoreDetailsId",
                table: "ProductUnit");

            migrationBuilder.AlterColumn<Guid>(
                name: "ProductUnitStatusId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "ExpiryDate",
                table: "ProductUnit",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "ProductDetailsId",
                table: "ProductUnit",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreDetailsId",
                table: "MainSalesClass",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreDetailsId",
                table: "MainPurchaseClass",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ProductDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MeasurementUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    UnitPrice = table.Column<decimal>(type: "decimal(16,2)", precision: 16, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductDetails_MeasurementUnit_MeasurementUnitId",
                        column: x => x.MeasurementUnitId,
                        principalTable: "MeasurementUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductDetails_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnit_ProductDetailsId",
                table: "ProductUnit",
                column: "ProductDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_StoreDetailsId",
                table: "MainSalesClass",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_StoreDetailsId",
                table: "MainPurchaseClass",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_MeasurementUnitId",
                table: "ProductDetails",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductDetails_StoreDetailsId",
                table: "ProductDetails",
                column: "StoreDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_MainPurchaseClass_StoreDetails_StoreDetailsId",
                table: "MainPurchaseClass",
                column: "StoreDetailsId",
                principalTable: "StoreDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MainSalesClass_StoreDetails_StoreDetailsId",
                table: "MainSalesClass",
                column: "StoreDetailsId",
                principalTable: "StoreDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_ProductDetails_ProductDetailsId",
                table: "ProductUnit",
                column: "ProductDetailsId",
                principalTable: "ProductDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnit_ProductUnitStatus_ProductUnitStatusId",
                table: "ProductUnit",
                column: "ProductUnitStatusId",
                principalTable: "ProductUnitStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
