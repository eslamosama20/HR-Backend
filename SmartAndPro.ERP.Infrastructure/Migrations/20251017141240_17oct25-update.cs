using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _17oct25update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "SaleInvoice");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Reservation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceId",
                table: "Reservation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RequestForQuotation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestForQuotation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RequestForQuotationItem",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RequestForQuotationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestForQuotationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestForQuotationItem_RequestForQuotation_RequestForQuotationId",
                        column: x => x.RequestForQuotationId,
                        principalTable: "RequestForQuotation",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservation_SaleInvoiceId",
                table: "Reservation",
                column: "SaleInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestForQuotationItem_RequestForQuotationId",
                table: "RequestForQuotationItem",
                column: "RequestForQuotationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservation_SaleInvoice_SaleInvoiceId",
                table: "Reservation",
                column: "SaleInvoiceId",
                principalTable: "SaleInvoice",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservation_SaleInvoice_SaleInvoiceId",
                table: "Reservation");

            migrationBuilder.DropTable(
                name: "RequestForQuotationItem");

            migrationBuilder.DropTable(
                name: "RequestForQuotation");

            migrationBuilder.DropIndex(
                name: "IX_Reservation_SaleInvoiceId",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Reservation");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceId",
                table: "Reservation");

            migrationBuilder.AddColumn<Guid>(
                name: "ReservationId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: true);
        }
    }
}
