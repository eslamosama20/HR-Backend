using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryPermits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryPermit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    PermitDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermitType = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Entry"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryPermit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryPermit_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryPermit_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PermitEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "money", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryPermitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PermitEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PermitEntry_InventoryPermit_InventoryPermitId",
                        column: x => x.InventoryPermitId,
                        principalTable: "InventoryPermit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermitEntry_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PermitEntry_MainSalesClass_MainSalesClassId",
                        column: x => x.MainSalesClassId,
                        principalTable: "MainSalesClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryPermit_MainAccountId",
                table: "InventoryPermit",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryPermit_StoreDetailsId",
                table: "InventoryPermit",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitEntry_InventoryPermitId",
                table: "PermitEntry",
                column: "InventoryPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitEntry_MainPurchaseClassId",
                table: "PermitEntry",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PermitEntry_MainSalesClassId",
                table: "PermitEntry",
                column: "MainSalesClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PermitEntry");

            migrationBuilder.DropTable(
                name: "InventoryPermit");
        }
    }
}
