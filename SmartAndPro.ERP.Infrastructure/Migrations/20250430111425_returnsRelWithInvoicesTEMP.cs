using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class returnsRelWithInvoicesTEMP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice");

            migrationBuilder.AddColumn<string>(
                name: "SalesInvoiceId",
                table: "SalesReturn",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerProfileId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<string>(
                name: "PurchaseInvoiceId",
                table: "PurchaseReturn",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice",
                column: "CustomerProfileId",
                principalTable: "CustomerProfile",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_CustomerProfile_CustomerProfileId",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "SalesInvoiceId",
                table: "SalesReturn");

            migrationBuilder.DropColumn(
                name: "PurchaseInvoiceId",
                table: "PurchaseReturn");

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
    }
}
