using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PurchaseAndSalesReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PurchaseReturnId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesReturnId",
                table: "Transaction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PurchaseReturn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturn_SupplierDetails_SupplierDetailsId",
                        column: x => x.SupplierDetailsId,
                        principalTable: "SupplierDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturn_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnTypes",
                columns: table => new
                {
                    Id = table.Column<short>(type: "smallint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturn",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReturnNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturn", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturn_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturn_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PurchaseReturnItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseReturnId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnItems_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnItems_PurchaseReturn_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseReturnItems_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SalesReturnItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SalesReturnId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesReturnItems_MainSalesClass_MainSalesClassId",
                        column: x => x.MainSalesClassId,
                        principalTable: "MainSalesClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesReturnItems_SalesReturn_SalesReturnId",
                        column: x => x.SalesReturnId,
                        principalTable: "SalesReturn",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PurchaseReturnId",
                table: "Transaction",
                column: "PurchaseReturnId",
                unique: true,
                filter: "[PurchaseReturnId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SalesReturnId",
                table: "Transaction",
                column: "SalesReturnId",
                unique: true,
                filter: "[SalesReturnId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturn_SupplierDetailsId",
                table: "PurchaseReturn",
                column: "SupplierDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturn_TransactionId",
                table: "PurchaseReturn",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_MainPurchaseClassId",
                table: "PurchaseReturnItems",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_PurchaseReturnId",
                table: "PurchaseReturnItems",
                column: "PurchaseReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_StoreDetailsId",
                table: "PurchaseReturnItems",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturn_CustomerProfileId",
                table: "SalesReturn",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturn_TransactionId",
                table: "SalesReturn",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnItems_MainSalesClassId",
                table: "SalesReturnItems",
                column: "MainSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnItems_SalesReturnId",
                table: "SalesReturnItems",
                column: "SalesReturnId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_PurchaseReturn_PurchaseReturnId",
                table: "Transaction",
                column: "PurchaseReturnId",
                principalTable: "PurchaseReturn",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_SalesReturn_SalesReturnId",
                table: "Transaction",
                column: "SalesReturnId",
                principalTable: "SalesReturn",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_PurchaseReturn_PurchaseReturnId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_SalesReturn_SalesReturnId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "PurchaseReturnItems");

            migrationBuilder.DropTable(
                name: "ReturnTypes");

            migrationBuilder.DropTable(
                name: "SalesReturnItems");

            migrationBuilder.DropTable(
                name: "PurchaseReturn");

            migrationBuilder.DropTable(
                name: "SalesReturn");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_PurchaseReturnId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SalesReturnId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "PurchaseReturnId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "SalesReturnId",
                table: "Transaction");
        }
    }
}
