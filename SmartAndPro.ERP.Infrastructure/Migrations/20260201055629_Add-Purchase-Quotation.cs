using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseQuotation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservationSerialNumberFormat",
                table: "SystemSetting",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservationSerialPrefix",
                table: "SystemSetting",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReservationSerialStartNumber",
                table: "SystemSetting",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceType",
                table: "SaleInvoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseQuotationId",
                table: "PurchaseInvoice",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethodType",
                table: "PaymentSchedule",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "PaymentSchedule",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PurchaseQuotationId",
                table: "Attachments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PurchaseQuotation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SupplierDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsFrontal = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
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
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseQuotation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseQuotation_SupplierDetails_SupplierDetailsId",
                        column: x => x.SupplierDetailsId,
                        principalTable: "SupplierDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PurchaseQuotationItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurchaseQuotationId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ItemNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MainPurchaseClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsTaxExempted = table.Column<bool>(type: "bit", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseQuotationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseQuotationItems_MainPurchaseClass_MainPurchaseClassId",
                        column: x => x.MainPurchaseClassId,
                        principalTable: "MainPurchaseClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseQuotationItems_PurchaseQuotation_PurchaseQuotationId",
                        column: x => x.PurchaseQuotationId,
                        principalTable: "PurchaseQuotation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoice_PurchaseQuotationId",
                table: "PurchaseInvoice",
                column: "PurchaseQuotationId",
                unique: true,
                filter: "[PurchaseQuotationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PurchaseQuotationId",
                table: "Attachments",
                column: "PurchaseQuotationId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuotation_SupplierDetailsId",
                table: "PurchaseQuotation",
                column: "SupplierDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuotationItems_MainPurchaseClassId",
                table: "PurchaseQuotationItems",
                column: "MainPurchaseClassId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseQuotationItems_PurchaseQuotationId",
                table: "PurchaseQuotationItems",
                column: "PurchaseQuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_PurchaseQuotation_PurchaseQuotationId",
                table: "Attachments",
                column: "PurchaseQuotationId",
                principalTable: "PurchaseQuotation",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoice_PurchaseQuotation_PurchaseQuotationId",
                table: "PurchaseInvoice",
                column: "PurchaseQuotationId",
                principalTable: "PurchaseQuotation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_PurchaseQuotation_PurchaseQuotationId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoice_PurchaseQuotation_PurchaseQuotationId",
                table: "PurchaseInvoice");

            migrationBuilder.DropTable(
                name: "PurchaseQuotationItems");

            migrationBuilder.DropTable(
                name: "PurchaseQuotation");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoice_PurchaseQuotationId",
                table: "PurchaseInvoice");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_PurchaseQuotationId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "ReservationSerialNumberFormat",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "ReservationSerialPrefix",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "ReservationSerialStartNumber",
                table: "SystemSetting");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceType",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "PurchaseQuotationId",
                table: "PurchaseInvoice");

            migrationBuilder.DropColumn(
                name: "PaymentMethodType",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "PaymentSchedule");

            migrationBuilder.DropColumn(
                name: "PurchaseQuotationId",
                table: "Attachments");
        }
    }
}
