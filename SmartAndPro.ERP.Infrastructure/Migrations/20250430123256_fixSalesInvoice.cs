using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class fixSalesInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerProfileId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice",
                column: "CustomerProfileId",
                principalTable: "CustomerProfile",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerProfileId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice",
                column: "CustomerProfileId",
                principalTable: "CustomerProfile",
                principalColumn: "Id");
        }
    }
}
