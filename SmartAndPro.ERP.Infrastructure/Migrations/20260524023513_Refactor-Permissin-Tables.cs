using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPermissinTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
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
