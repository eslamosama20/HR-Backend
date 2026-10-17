using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectFormFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "ReceiptConnection",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentApprovalDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignmentNumber",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckAmount",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckNumber",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CheckReceiptDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearingDetails",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CommissionPercentage",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompanyDetailsId",
                table: "Projects",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExecutionDuration",
                table: "Projects",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExecutionDurationUnit",
                table: "Projects",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "QuoteDate",
                table: "Projects",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QuoteNumber",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShareToken",
                table: "Projects",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupervisingEntityName",
                table: "Projects",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptConnection_ProjectId",
                table: "ReceiptConnection",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CompanyDetailsId",
                table: "Projects",
                column: "CompanyDetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_CompanyDetails_CompanyDetailsId",
                table: "Projects",
                column: "CompanyDetailsId",
                principalTable: "CompanyDetails",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptConnection_Projects_ProjectId",
                table: "ReceiptConnection",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
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
