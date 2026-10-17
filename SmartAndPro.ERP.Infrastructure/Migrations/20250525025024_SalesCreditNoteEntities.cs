using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SalesCreditNoteEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_CreditNotes_CreditNoteId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_CreditNotes_CreditNoteId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "CreditNoteAccounts");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_CreditNoteId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_CreditNoteId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "CreditNoteId",
                table: "Transaction",
                newName: "SalesCreditNoteId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "SaleItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                table: "SaleItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "SaleItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPrice",
                table: "SaleItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "SaleItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "SaleInvoice",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddColumn<Guid>(
                name: "SalesCreditNoteId",
                table: "Attachments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SalesCreditNote",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AdjustmentReason = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SalesCreditNoteNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsCleared = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    InvoiceHash = table.Column<string>(type: "nvarchar(999)", maxLength: 999, nullable: true),
                    GeneratedQR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SaleInvoiceId1 = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCreditNote", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesCreditNote_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesCreditNote_SaleInvoice_SaleInvoiceId",
                        column: x => x.SaleInvoiceId,
                        principalTable: "SaleInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesCreditNote_SaleInvoice_SaleInvoiceId1",
                        column: x => x.SaleInvoiceId1,
                        principalTable: "SaleInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SalesCreditNote_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SalesCreditNoteItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainSalesClassId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NoOfNights = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SalesCreditNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesCreditNoteItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SalesCreditNoteItems_MainSalesClass_MainSalesClassId",
                        column: x => x.MainSalesClassId,
                        principalTable: "MainSalesClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SalesCreditNoteItems_SalesCreditNote_SalesCreditNoteId",
                        column: x => x.SalesCreditNoteId,
                        principalTable: "SalesCreditNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_SalesCreditNoteId",
                table: "Transaction",
                column: "SalesCreditNoteId",
                unique: true,
                filter: "[SalesCreditNoteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_SalesCreditNoteId",
                table: "Attachments",
                column: "SalesCreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCreditNote_CustomerProfileId",
                table: "SalesCreditNote",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCreditNote_SaleInvoiceId",
                table: "SalesCreditNote",
                column: "SaleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCreditNote_SaleInvoiceId1",
                table: "SalesCreditNote",
                column: "SaleInvoiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCreditNote_TransactionId",
                table: "SalesCreditNote",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCreditNoteItems_MainSalesClassId",
                table: "SalesCreditNoteItems",
                column: "MainSalesClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesCreditNoteItems_SalesCreditNoteId",
                table: "SalesCreditNoteItems",
                column: "SalesCreditNoteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_SalesCreditNote_SalesCreditNoteId",
                table: "Attachments",
                column: "SalesCreditNoteId",
                principalTable: "SalesCreditNote",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_SalesCreditNote_SalesCreditNoteId",
                table: "Transaction",
                column: "SalesCreditNoteId",
                principalTable: "SalesCreditNote",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_SalesCreditNote_SalesCreditNoteId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_SalesCreditNote_SalesCreditNoteId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "SalesCreditNoteItems");

            migrationBuilder.DropTable(
                name: "SalesCreditNote");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_SalesCreditNoteId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_SalesCreditNoteId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "SalesCreditNoteId",
                table: "Attachments");

            migrationBuilder.RenameColumn(
                name: "SalesCreditNoteId",
                table: "Transaction",
                newName: "CreditNoteId");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalPrice",
                table: "SaleItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                table: "SaleItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "SaleItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "NetPrice",
                table: "SaleItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "SaleItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "SaleInvoice",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNotes_CustomerProfile_CustomerProfileId",
                        column: x => x.CustomerProfileId,
                        principalTable: "CustomerProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CreditNotes_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CreditNoteAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNoteAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditNoteAccounts_CreditNotes_CreditNoteId",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditNoteAccounts_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_CreditNoteId",
                table: "Transaction",
                column: "CreditNoteId",
                unique: true,
                filter: "[CreditNoteId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CreditNoteId",
                table: "Attachments",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteAccounts_CreditNoteId",
                table: "CreditNoteAccounts",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNoteAccounts_MainAccountId",
                table: "CreditNoteAccounts",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_CustomerProfileId",
                table: "CreditNotes",
                column: "CustomerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_ReferenceNumber",
                table: "CreditNotes",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_TransactionId",
                table: "CreditNotes",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_CreditNotes_CreditNoteId",
                table: "Attachments",
                column: "CreditNoteId",
                principalTable: "CreditNotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_CreditNotes_CreditNoteId",
                table: "Transaction",
                column: "CreditNoteId",
                principalTable: "CreditNotes",
                principalColumn: "Id");
        }
    }
}
