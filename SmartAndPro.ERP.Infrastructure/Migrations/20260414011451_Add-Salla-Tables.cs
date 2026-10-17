using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSallaTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SallaAuthTokens");

            migrationBuilder.DropTable(
                name: "SallaOrderItems");

            migrationBuilder.DropTable(
                name: "SallaProductMappings");

            migrationBuilder.DropTable(
                name: "SallaSyncLogs");

            migrationBuilder.DropTable(
                name: "SallaOrders");
        }
    }
}
