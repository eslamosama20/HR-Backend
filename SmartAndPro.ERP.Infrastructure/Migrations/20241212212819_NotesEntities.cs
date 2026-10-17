using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NotesEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreditNoteId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DebitNoteId",
                table: "Transaction",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "DebitNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SupplierDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitNotes_SupplierDetails_SupplierDetailsId",
                        column: x => x.SupplierDetailsId,
                        principalTable: "SupplierDetails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DebitNotes_Transaction_TransactionId",
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
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreditNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "DebitNoteAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DebitNoteId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitNoteAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitNoteAccounts_DebitNotes_DebitNoteId",
                        column: x => x.DebitNoteId,
                        principalTable: "DebitNotes",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitNoteAccounts_MainAccount_MainAccountId",
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
                name: "IX_Transaction_DebitNoteId",
                table: "Transaction",
                column: "DebitNoteId",
                unique: true,
                filter: "[DebitNoteId] IS NOT NULL");

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

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteAccounts_DebitNoteId",
                table: "DebitNoteAccounts",
                column: "DebitNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNoteAccounts_MainAccountId",
                table: "DebitNoteAccounts",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_ReferenceNumber",
                table: "DebitNotes",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_SupplierDetailsId",
                table: "DebitNotes",
                column: "SupplierDetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitNotes_TransactionId",
                table: "DebitNotes",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_CreditNotes_CreditNoteId",
                table: "Transaction",
                column: "CreditNoteId",
                principalTable: "CreditNotes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_DebitNotes_DebitNoteId",
                table: "Transaction",
                column: "DebitNoteId",
                principalTable: "DebitNotes",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_CreditNotes_CreditNoteId",
                table: "Transaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_DebitNotes_DebitNoteId",
                table: "Transaction");

            migrationBuilder.DropTable(
                name: "CreditNoteAccounts");

            migrationBuilder.DropTable(
                name: "DebitNoteAccounts");

            migrationBuilder.DropTable(
                name: "CreditNotes");

            migrationBuilder.DropTable(
                name: "DebitNotes");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_CreditNoteId",
                table: "Transaction");

            migrationBuilder.DropIndex(
                name: "IX_Transaction_DebitNoteId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "CreditNoteId",
                table: "Transaction");

            migrationBuilder.DropColumn(
                name: "DebitNoteId",
                table: "Transaction");
        }
    }
}
