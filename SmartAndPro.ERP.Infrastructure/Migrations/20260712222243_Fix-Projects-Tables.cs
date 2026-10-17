using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixProjectsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_CompanyDetails_CompanyDetailsId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptConnection_Projects_ProjectId",
                table: "ReceiptConnection");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptConnection_ProjectId",
                table: "ReceiptConnection");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CompanyDetailsId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "ReceiptConnection");

            migrationBuilder.DropColumn(
                name: "AssignmentApprovalDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "AssignmentNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CheckAmount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CheckNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CheckReceiptDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ClearingDetails",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CommissionPercentage",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CompanyDetailsId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ExecutionDuration",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ExecutionDurationUnit",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "QuoteDate",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "QuoteNumber",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ShareToken",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "SupervisingEntityName",
                table: "Projects");
        }
    }
}
