using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StoreStatusNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreDetails_StoreStatus_StoreStatusId",
                table: "StoreDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreStatusId",
                table: "StoreDetails",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_StoreDetails_StoreStatus_StoreStatusId",
                table: "StoreDetails",
                column: "StoreStatusId",
                principalTable: "StoreStatus",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoreDetails_StoreStatus_StoreStatusId",
                table: "StoreDetails");

            migrationBuilder.AlterColumn<Guid>(
                name: "StoreStatusId",
                table: "StoreDetails",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StoreDetails_StoreStatus_StoreStatusId",
                table: "StoreDetails",
                column: "StoreStatusId",
                principalTable: "StoreStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
