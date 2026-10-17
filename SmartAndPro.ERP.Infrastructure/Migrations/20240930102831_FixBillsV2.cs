using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBillsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Assets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Treasuries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Treasuries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BillOfExchanges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreasuryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillOfExchangeCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfExchanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfExchanges_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillOfSales",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SellerId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TreasuryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillOfSalesCreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillOfSales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillOfSales_Assets_AssetId",
                        column: x => x.AssetId,
                        principalTable: "Assets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BillsAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierBankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BillOfExchangeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BillOfSalesId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillsAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillsAccounts_BillOfExchanges_BillOfExchangeId",
                        column: x => x.BillOfExchangeId,
                        principalTable: "BillOfExchanges",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_BillsAccounts_BillOfSales_BillOfSalesId",
                        column: x => x.BillOfSalesId,
                        principalTable: "BillOfSales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_BillOfExchanges_AssetId",
                table: "BillOfExchanges",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_BillOfSales_AssetId",
                table: "BillOfSales",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_BillsAccounts_BillOfExchangeId",
                table: "BillsAccounts",
                column: "BillOfExchangeId");

            migrationBuilder.CreateIndex(
                name: "IX_BillsAccounts_BillOfSalesId",
                table: "BillsAccounts",
                column: "BillOfSalesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillsAccounts");

            migrationBuilder.DropTable(
                name: "Treasuries");

            migrationBuilder.DropTable(
                name: "BillOfExchanges");

            migrationBuilder.DropTable(
                name: "BillOfSales");

            migrationBuilder.DropTable(
                name: "Assets");
        }
    }
}
