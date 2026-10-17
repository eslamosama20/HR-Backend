using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InventoryOperationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "MainPurchaseClassId",
                table: "PermitEntry",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "InventoryOperation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    OperationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OperationSource = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PurchaseInvoiceId = table.Column<int>(type: "int", nullable: true),
                    InventoryPermitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseReturnId = table.Column<int>(type: "int", nullable: true),
                    SupplierDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryOperation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryOperation_InventoryPermit_InventoryPermitId",
                        column: x => x.InventoryPermitId,
                        principalTable: "InventoryPermit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryOperation_PurchaseInvoice_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryOperation_PurchaseReturn_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryOperation_SupplierDetails_SupplierDetailsId",
                        column: x => x.SupplierDetailsId,
                        principalTable: "SupplierDetails",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_InventoryOperation_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "OperationEntry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "money", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryOperationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperationEntry_InventoryOperation_InventoryOperationId",
                        column: x => x.InventoryOperationId,
                        principalTable: "InventoryOperation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OperationEntry_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OperationEntry_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOperation_InventoryPermitId",
                table: "InventoryOperation",
                column: "InventoryPermitId",
                unique: true,
                filter: "[InventoryPermitId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOperation_PurchaseInvoiceId",
                table: "InventoryOperation",
                column: "PurchaseInvoiceId",
                unique: true,
                filter: "[PurchaseInvoiceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOperation_PurchaseReturnId",
                table: "InventoryOperation",
                column: "PurchaseReturnId",
                unique: true,
                filter: "[PurchaseReturnId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOperation_SupplierDetailsId",
                table: "InventoryOperation",
                column: "SupplierDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryOperation_TransactionId",
                table: "InventoryOperation",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_OperationEntry_InventoryOperationId",
                table: "OperationEntry",
                column: "InventoryOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationEntry_MainPurchaseClassId",
                table: "OperationEntry",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationEntry_StoreDetailsId",
                table: "OperationEntry",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OperationEntry");

            migrationBuilder.DropTable(
                name: "InventoryOperation");

            migrationBuilder.AlterColumn<Guid>(
                name: "MainPurchaseClassId",
                table: "PermitEntry",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }
    }
}
