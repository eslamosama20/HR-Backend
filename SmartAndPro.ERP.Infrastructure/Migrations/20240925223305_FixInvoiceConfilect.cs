using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixInvoiceConfilect : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_CustomerProfile_CustomerProfileId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_VendorProfile_VendorProfileId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "VendorProfile");

            migrationBuilder.DropTable(
                name: "Invoice");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_CustomerProfileId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_VendorProfileId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "CustomerProfileId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "VendorProfileId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "StoreDetails");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "StoreDetails");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "StoreDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "SupplierDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "StoreDetails",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "CustomerProfile",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PurchaseInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoice_SupplierDetails_SupplierDetailsId",
                        column: x => x.SupplierDetailsId,
                        principalTable: "SupplierDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleInvoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
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
                    table.PrimaryKey("PK_SaleInvoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_PurchaseItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_PurchaseInvoice_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseItems_StoreDetails_StoreDetailsId",
                        column: x => x.StoreDetailsId,
                        principalTable: "StoreDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SaleItems_SaleInvoice_SaleInvoiceId",
                        column: x => x.SaleInvoiceId,
                        principalTable: "SaleInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierDetails_MainAccountId",
                table: "SupplierDetails",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_StoreDetails_MainAccountId",
                table: "StoreDetails",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerProfile_MainAccountId",
                table: "CustomerProfile",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_SupplierDetailsId",
                table: "PurchaseInvoice",
                column: "SupplierDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_MainPurchaseClassId",
                table: "PurchaseItems",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_PurchaseInvoiceId",
                table: "PurchaseItems",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseItems_StoreDetailsId",
                table: "PurchaseItems",
                column: "StoreDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_CustomerProfileId",
                table: "SaleInvoice",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_MainPurchaseClassId",
                table: "SaleItems",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleInvoiceId",
                table: "SaleItems",
                column: "SaleInvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerProfile_MainAccount_MainAccountId",
                table: "CustomerProfile",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreDetails_MainAccount_MainAccountId",
                table: "StoreDetails",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierDetails_MainAccount_MainAccountId",
                table: "SupplierDetails",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomerProfile_MainAccount_MainAccountId",
                table: "CustomerProfile");

            migrationBuilder.DropForeignKey(
                name: "FK_StoreDetails_MainAccount_MainAccountId",
                table: "StoreDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierDetails_MainAccount_MainAccountId",
                table: "SupplierDetails");

            migrationBuilder.DropTable(
                name: "PurchaseItems");

            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "PurchaseInvoice");

            migrationBuilder.DropTable(
                name: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SupplierDetails_MainAccountId",
                table: "SupplierDetails");

            migrationBuilder.DropIndex(
                name: "IX_StoreDetails_MainAccountId",
                table: "StoreDetails");

            migrationBuilder.DropIndex(
                name: "IX_CustomerProfile_MainAccountId",
                table: "CustomerProfile");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "SupplierDetails");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "StoreDetails");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "CustomerProfile");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerProfileId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VendorProfileId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "StoreDetails",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "StoreDetails",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "StoreDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Invoice",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceTypeId = table.Column<short>(type: "smallint", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoice_InvoiceTypes_InvoiceTypeId",
                        column: x => x.InvoiceTypeId,
                        principalTable: "InvoiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VendorProfile",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArabicName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BankAccountNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EnglishName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IBAN = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TaxNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendorProfile", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ItemNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    StoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SupplierId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                name: "IX_Transaction_CustomerProfileId",
                table: "Transaction",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_VendorProfileId",
                table: "Transaction",
                column: "VendorProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_InvoiceTypeId",
                table: "Invoice",
                column: "InvoiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_InvoiceId",
                table: "Items",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfile_ArabicName",
                table: "VendorProfile",
                column: "ArabicName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VendorProfile_EnglishName",
                table: "VendorProfile",
                column: "EnglishName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_CustomerProfile_CustomerProfileId",
                table: "Transaction",
                column: "CustomerProfileId",
                principalTable: "CustomerProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_VendorProfile_VendorProfileId",
                table: "Transaction",
                column: "VendorProfileId",
                principalTable: "VendorProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
