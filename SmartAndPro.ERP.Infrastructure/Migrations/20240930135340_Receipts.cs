using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Receipts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaycheckReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaycheckReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaycheckReceipts_Treasuries_TreasuryId",
                        column: x => x.TreasuryId,
                        principalTable: "Treasuries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentReceipts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferenceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ReceiptDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TreasuryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReceipts_Treasuries_TreasuryId",
                        column: x => x.TreasuryId,
                        principalTable: "Treasuries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaycheckReceiptAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaycheckReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaycheckReceiptAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaycheckReceiptAccounts_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaycheckReceiptAccounts_PaycheckReceipts_PaycheckReceiptId",
                        column: x => x.PaycheckReceiptId,
                        principalTable: "PaycheckReceipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PaymentReceiptAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    Tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MainAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentReceiptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentReceiptAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentReceiptAccounts_MainAccount_MainAccountId",
                        column: x => x.MainAccountId,
                        principalTable: "MainAccount",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PaymentReceiptAccounts_PaymentReceipts_PaymentReceiptId",
                        column: x => x.PaymentReceiptId,
                        principalTable: "PaymentReceipts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceiptAccounts_MainAccountId",
                table: "PaycheckReceiptAccounts",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceiptAccounts_PaycheckReceiptId",
                table: "PaycheckReceiptAccounts",
                column: "PaycheckReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_ReferenceNumber",
                table: "PaycheckReceipts",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaycheckReceipts_TreasuryId",
                table: "PaycheckReceipts",
                column: "TreasuryId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptAccounts_MainAccountId",
                table: "PaymentReceiptAccounts",
                column: "MainAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceiptAccounts_PaymentReceiptId",
                table: "PaymentReceiptAccounts",
                column: "PaymentReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_ReferenceNumber",
                table: "PaymentReceipts",
                column: "ReferenceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentReceipts_TreasuryId",
                table: "PaymentReceipts",
                column: "TreasuryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaycheckReceiptAccounts");

            migrationBuilder.DropTable(
                name: "PaymentReceiptAccounts");

            migrationBuilder.DropTable(
                name: "PaycheckReceipts");

            migrationBuilder.DropTable(
                name: "PaymentReceipts");
        }
    }
}
