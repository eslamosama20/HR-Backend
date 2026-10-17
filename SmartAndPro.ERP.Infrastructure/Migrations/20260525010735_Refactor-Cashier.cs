using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCashier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierInventoryMovements");

            migrationBuilder.DropTable(
                name: "CheckoutIdempotencyRecords");

            migrationBuilder.DropColumn(
                name: "PaymentReference",
                table: "CashierPayment");

            migrationBuilder.DropColumn(
                name: "PaymentStatus",
                table: "CashierPayment");

            migrationBuilder.DropColumn(
                name: "StatusChangedAtUtc",
                table: "CashierPayment");

            migrationBuilder.DropColumn(
                name: "StatusReason",
                table: "CashierPayment");
        }
    }
}
