using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectsV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectPriceQuotes_SaleInvoice_SaleInvoiceId",
                table: "ProjectPriceQuotes");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_CustomerDepartments_CustomerDepartmentId",
                table: "SaleInvoice");

            migrationBuilder.DropForeignKey(
                name: "FK_SaleInvoice_Projects_ProjectId",
                table: "SaleInvoice");

            migrationBuilder.DropTable(
                name: "ProjectExecutionMilestones");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_CustomerDepartmentId",
                table: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_SaleInvoice_ProjectId",
                table: "SaleInvoice");

            migrationBuilder.DropIndex(
                name: "IX_ProjectPriceQuotes_SaleInvoiceId",
                table: "ProjectPriceQuotes");

            migrationBuilder.DropColumn(
                name: "CustomerDepartmentId",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "SaleInvoice");

            migrationBuilder.DropColumn(
                name: "SaleInvoiceId",
                table: "ProjectPriceQuotes");

            migrationBuilder.DropColumn(
                name: "IsForProjects",
                table: "DetailSalesClass");
        }
    }
}
