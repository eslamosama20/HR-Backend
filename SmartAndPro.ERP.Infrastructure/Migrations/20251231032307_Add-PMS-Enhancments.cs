using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPMSEnhancments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ExecutionAmount",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ProfitPercentage",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Projects",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExecutionAmount",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ProfitPercentage",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Projects");
        }
    }
}
