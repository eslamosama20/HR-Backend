using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HeldOrderLines");

            migrationBuilder.DropTable(
                name: "PaymentProviderEvents");

            migrationBuilder.DropTable(
                name: "HeldOrders");

            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "CheckoutIdempotencyRecords");
        }
    }
}
