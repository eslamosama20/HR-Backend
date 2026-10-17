using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDispalyOrderAndUpdateCachir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "ReservationPrintTableTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "DetailAccountId",
                table: "MainAccount",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CashierAuditLog");

            migrationBuilder.DropTable(
                name: "CashierStockReservation");

            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "ReservationPrintTableTemplates");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProductUnit");

            migrationBuilder.AlterColumn<Guid>(
                name: "DetailAccountId",
                table: "MainAccount",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
