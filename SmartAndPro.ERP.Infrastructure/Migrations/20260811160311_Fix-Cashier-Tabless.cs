using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCashierTabless : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<int>(
                name: "StockAlertLimit",
                table: "CashierSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ItemsCount",
                table: "CashierRefund",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosedByUserGuid",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "ClosedByUserName",
                table: "CashierSessions");

            migrationBuilder.DropColumn(
                name: "StockAlertLimit",
                table: "CashierSessions");

            migrationBuilder.AlterColumn<int>(
                name: "ItemsCount",
                table: "CashierRefund",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);
        }
    }
}
