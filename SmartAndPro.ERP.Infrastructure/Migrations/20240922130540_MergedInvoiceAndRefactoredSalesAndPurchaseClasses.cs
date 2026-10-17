using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MergedInvoiceAndRefactoredSalesAndPurchaseClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClassType",
                table: "MainSalesClass",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "MeasurementUnitId",
                table: "MainSalesClass",
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
                name: "MeasurementUnitId",
                table: "MainPurchaseClass",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "StoreDetailsId",
                table: "MainPurchaseClass",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Logo",
                table: "CompanyDetails",
                type: "nvarchar(600)",
                maxLength: 600,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CostCenter",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CostCenter", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Items_Invoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_MeasurementUnitId",
                table: "MainSalesClass",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MainSalesClass_StoreDetailsId",
                table: "MainSalesClass",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_MeasurementUnitId",
                table: "MainPurchaseClass",
                column: "MeasurementUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MainPurchaseClass_StoreDetailsId",
                table: "MainPurchaseClass",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InvoiceId",
                table: "Items",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_MainPurchaseClass_MeasurementUnit_MeasurementUnitId",
                table: "MainPurchaseClass",
                column: "MeasurementUnitId",
                principalTable: "MeasurementUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MainPurchaseClass_StoreDetails_StoreDetailsId",
                table: "MainPurchaseClass",
                column: "StoreDetailsId",
                principalTable: "StoreDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MainSalesClass_MeasurementUnit_MeasurementUnitId",
                table: "MainSalesClass",
                column: "MeasurementUnitId",
                principalTable: "MeasurementUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MainSalesClass_StoreDetails_StoreDetailsId",
                table: "MainSalesClass",
                column: "StoreDetailsId",
                principalTable: "StoreDetails",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MainPurchaseClass_MeasurementUnit_MeasurementUnitId",
                table: "MainPurchaseClass");

            migrationBuilder.DropForeignKey(
                name: "FK_MainPurchaseClass_StoreDetails_StoreDetailsId",
                table: "MainPurchaseClass");

            migrationBuilder.DropForeignKey(
                name: "FK_MainSalesClass_MeasurementUnit_MeasurementUnitId",
                table: "MainSalesClass");

            migrationBuilder.DropForeignKey(
                name: "FK_MainSalesClass_StoreDetails_StoreDetailsId",
                table: "MainSalesClass");

            migrationBuilder.DropTable(
                name: "CostCenter");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_MeasurementUnitId",
                table: "MainSalesClass");

            migrationBuilder.DropIndex(
                name: "IX_MainSalesClass_StoreDetailsId",
                table: "MainSalesClass");

            migrationBuilder.DropIndex(
                name: "IX_MainPurchaseClass_MeasurementUnitId",
                table: "MainPurchaseClass");

            migrationBuilder.DropIndex(
                name: "IX_MainPurchaseClass_StoreDetailsId",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "ClassType",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "MeasurementUnitId",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "StoreDetailsId",
                table: "MainSalesClass");

            migrationBuilder.DropColumn(
                name: "MeasurementUnitId",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "StoreDetailsId",
                table: "MainPurchaseClass");

            migrationBuilder.DropColumn(
                name: "Logo",
                table: "CompanyDetails");
        }
    }
}
