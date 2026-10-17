using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStagnantInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StagnantInventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    LastMovementDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AddedToStagnantDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<int>(type: "int", nullable: false),
                    StagnantPeriodMonths = table.Column<int>(type: "int", nullable: true),
                    IsAutoAdded = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AddedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDisposed = table.Column<bool>(type: "bit", nullable: false),
                    DisposedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisposalMethod = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StagnantInventory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StagnantInventory_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StagnantInventory_MainSalesClass_MainSalesClassId",
                        column: x => x.MainSalesClassId,
                        principalTable: "MainSalesClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StagnantInventory_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StagnantInventory_MainPurchaseClassId",
                table: "StagnantInventory",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StagnantInventory_MainSalesClassId",
                table: "StagnantInventory",
                column: "MainSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_StagnantInventory_StoreDetailsId",
                table: "StagnantInventory",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StagnantInventory");
        }
    }
}
