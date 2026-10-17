using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomReminderDays",
                table: "InventoryPermit",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastReminderDate",
                table: "InventoryPermit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextReminderDate",
                table: "InventoryPermit",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReminderEnabled",
                table: "InventoryPermit",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReminderFrequency",
                table: "InventoryPermit",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomReminderDays",
                table: "InventoryPermit");

            migrationBuilder.DropColumn(
                name: "LastReminderDate",
                table: "InventoryPermit");

            migrationBuilder.DropColumn(
                name: "NextReminderDate",
                table: "InventoryPermit");

            migrationBuilder.DropColumn(
                name: "ReminderEnabled",
                table: "InventoryPermit");

            migrationBuilder.DropColumn(
                name: "ReminderFrequency",
                table: "InventoryPermit");
        }
    }
}
