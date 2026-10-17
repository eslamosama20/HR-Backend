using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectMilestones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /*
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerDepartmentId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProjectId",
                table: "SaleInvoice",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SaleInvoiceId",
                table: "ProjectPriceQuotes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsForProjects",
                table: "DetailSalesClass",
                type: "bit",
                nullable: false,
                defaultValue: false);
            */

            migrationBuilder.CreateTable(
                name: "ProjectExecutionMilestones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MilestoneName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Duration = table.Column<int>(type: "int", nullable: false),
                    DurationUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SequenceOrder = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TargetEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectExecutionMilestones", x => x.Id);
                    table.ForeignKey(
                         name: "FK_ProjectExecutionMilestones_Projects_ProjectId",
                         column: x => x.ProjectId,
                         principalTable: "Projects",
                         principalColumn: "Id",
                         onDelete: ReferentialAction.Cascade);
                });

            /*
            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_CustomerDepartmentId",
                table: "SaleInvoice",
                column: "CustomerDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleInvoice_ProjectId",
                table: "SaleInvoice",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectPriceQuotes_SaleInvoiceId",
                table: "ProjectPriceQuotes",
                column: "SaleInvoiceId");
            */

            migrationBuilder.CreateIndex(
                name: "IX_ProjectExecutionMilestones_ProjectId",
                table: "ProjectExecutionMilestones",
                column: "ProjectId");

            /*
            migrationBuilder.AddForeignKey(
                name: "FK_ProjectPriceQuotes_SaleInvoice_SaleInvoiceId",
                table: "ProjectPriceQuotes",
                column: "SaleInvoiceId",
                principalTable: "SaleInvoice",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_CustomerDepartments_CustomerDepartmentId",
                table: "SaleInvoice",
                column: "CustomerDepartmentId",
                principalTable: "CustomerDepartments",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SaleInvoice_Projects_ProjectId",
                table: "SaleInvoice",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id");
            */
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
