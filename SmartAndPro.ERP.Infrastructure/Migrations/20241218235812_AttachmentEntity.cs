using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttachmentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Attachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InventoryPermitId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PurchaseInvoiceId = table.Column<int>(type: "int", nullable: true),
                    SaleInvoiceId = table.Column<int>(type: "int", nullable: true),
                    SalesReturnId = table.Column<int>(type: "int", nullable: true),
                    PurchaseReturnId = table.Column<int>(type: "int", nullable: true),
                    PaymentReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PaycheckReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DebitNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreditNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InventoryStocktakingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Attachments_CreditNotes_CreditNoteId",
                        column: x => x.CreditNoteId,
                        principalTable: "CreditNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_DebitNotes_DebitNoteId",
                        column: x => x.DebitNoteId,
                        principalTable: "DebitNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_InventoryPermit_InventoryPermitId",
                        column: x => x.InventoryPermitId,
                        principalTable: "InventoryPermit",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_InventoryStocktaking_InventoryStocktakingId",
                        column: x => x.InventoryStocktakingId,
                        principalTable: "InventoryStocktaking",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_PaycheckReceipts_PaycheckReceiptId",
                        column: x => x.PaycheckReceiptId,
                        principalTable: "PaycheckReceipts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_PaymentReceipts_PaymentReceiptId",
                        column: x => x.PaymentReceiptId,
                        principalTable: "PaymentReceipts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_PurchaseInvoice_PurchaseInvoiceId",
                        column: x => x.PurchaseInvoiceId,
                        principalTable: "PurchaseInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_PurchaseReturn_PurchaseReturnId",
                        column: x => x.PurchaseReturnId,
                        principalTable: "PurchaseReturn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_Reservation_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "Reservation",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_SaleInvoice_SaleInvoiceId",
                        column: x => x.SaleInvoiceId,
                        principalTable: "SaleInvoice",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_SalesReturn_SalesReturnId",
                        column: x => x.SalesReturnId,
                        principalTable: "SalesReturn",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Attachments_Transaction_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transaction",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_CreditNoteId",
                table: "Attachments",
                column: "CreditNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_DebitNoteId",
                table: "Attachments",
                column: "DebitNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_InventoryPermitId",
                table: "Attachments",
                column: "InventoryPermitId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_InventoryStocktakingId",
                table: "Attachments",
                column: "InventoryStocktakingId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PaycheckReceiptId",
                table: "Attachments",
                column: "PaycheckReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PaymentReceiptId",
                table: "Attachments",
                column: "PaymentReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PurchaseInvoiceId",
                table: "Attachments",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_PurchaseReturnId",
                table: "Attachments",
                column: "PurchaseReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_ReservationId",
                table: "Attachments",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_SaleInvoiceId",
                table: "Attachments",
                column: "SaleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_SalesReturnId",
                table: "Attachments",
                column: "SalesReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_TransactionId",
                table: "Attachments",
                column: "TransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachments");
        }
    }
}
