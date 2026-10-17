using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClosedByToCashierSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClosedByUserGuid",
                table: "CashierSessions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClosedByUserName",
                table: "CashierSessions",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);
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
        }
    }
}
