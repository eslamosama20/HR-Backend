using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCachierSessionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CashierSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false),
                    SessionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CashierUserGuid = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CashierUserName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    OpeningDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActualBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TreasuryId = table.Column<int>(type: "int", nullable: true),
                    StoreDetailsId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TotalSales = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalRefunds = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCash = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCard = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalCredit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TransactionsCount = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashierSessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CashierSessions_CashierUserGuid",
                table: "CashierSessions",
                column: "CashierUserGuid");

            migrationBuilder.CreateIndex(
                name: "IX_CashierSessions_OpeningDateUtc",
                table: "CashierSessions",
                column: "OpeningDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CashierSessions_SessionId",
                table: "CashierSessions",
                column: "SessionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CashierSessions_Status",
                table: "CashierSessions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CashierSessions_StoreDetailsId",
                table: "CashierSessions",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierSessions");
        }
    }
}
