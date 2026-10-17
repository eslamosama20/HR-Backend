using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TreasuryRelWithMainAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MainAccountId",
                table: "Treasuries",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Treasuries_MainAccountId",
                table: "Treasuries",
                column: "MainAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Treasuries_MainAccount_MainAccountId",
                table: "Treasuries",
                column: "MainAccountId",
                principalTable: "MainAccount",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Treasuries_MainAccount_MainAccountId",
                table: "Treasuries");

            migrationBuilder.DropIndex(
                name: "IX_Treasuries_MainAccountId",
                table: "Treasuries");

            migrationBuilder.DropColumn(
                name: "MainAccountId",
                table: "Treasuries");
        }
    }
}
