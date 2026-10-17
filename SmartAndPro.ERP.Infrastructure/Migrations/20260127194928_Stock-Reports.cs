using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StockReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DamagedInventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    DamageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReportedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamagedInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DamagedInventory_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DamagedInventory_MainSalesClass_MainSalesClassId",
                        column: x => x.MainSalesClassId,
                        principalTable: "MainSalesClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DamagedInventory_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamagedInventory_MainPurchaseClassId",
                table: "DamagedInventory",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DamagedInventory_MainSalesClassId",
                table: "DamagedInventory",
                column: "MainSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_DamagedInventory_StoreDetailsId",
                table: "DamagedInventory",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamagedInventory");
        }
    }
}
