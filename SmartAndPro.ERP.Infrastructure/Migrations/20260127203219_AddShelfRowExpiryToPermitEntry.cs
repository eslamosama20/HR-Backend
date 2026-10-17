using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShelfRowExpiryToPermitEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpiryDate",
                table: "PermitEntry",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RowNumber",
                table: "PermitEntry",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShelfNumber",
                table: "PermitEntry",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpiryDate",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "RowNumber",
                table: "PermitEntry");

            migrationBuilder.DropColumn(
                name: "ShelfNumber",
                table: "PermitEntry");
        }
    }
}
