using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StocktakingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InventoryStocktaking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StocktakingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryStocktaking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryStocktaking_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryStocktaking_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "StocktakingEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReportedQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryStocktakingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StocktakingEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StocktakingEntry_InventoryStocktaking_InventoryStocktakingId",
                        column: x => x.InventoryStocktakingId,
                        principalTable: "InventoryStocktaking",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StocktakingEntry_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktaking_StoreDetailsId",
                table: "InventoryStocktaking",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryStocktaking_TransactionId",
                table: "InventoryStocktaking",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakingEntry_InventoryStocktakingId",
                table: "StocktakingEntry",
                column: "InventoryStocktakingId");

            migrationBuilder.CreateIndex(
                name: "IX_StocktakingEntry_MainPurchaseClassId",
                table: "StocktakingEntry",
                column: "MainPurchaseClassId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StocktakingEntry");

            migrationBuilder.DropTable(
                name: "InventoryStocktaking");
        }
    }
}
