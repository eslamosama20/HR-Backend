using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCashierSessionSecurityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TerminalId",
                table: "CashierSessions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCashIn",
                table: "CashierSessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalCashOut",
                table: "CashierSessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalDiscounts",
                table: "CashierSessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalTax",
                table: "CashierSessions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_CashierSessions_TerminalId",
                table: "CashierSessions",
                column: "TerminalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CashierSessions_TerminalId",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "TerminalId",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "TotalCashIn",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "TotalCashOut",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "TotalDiscounts",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "TotalTax",
                table: "CashierSessions");
        }
    }
}
