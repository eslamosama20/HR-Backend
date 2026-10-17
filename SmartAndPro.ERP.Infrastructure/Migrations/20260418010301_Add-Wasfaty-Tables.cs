using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWasfatyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WasafatyAuthTokens");

            migrationBuilder.DropTable(
                name: "WasafatyPrescriptionItems");

            migrationBuilder.DropTable(
                name: "WasafatySyncLogs");

            migrationBuilder.DropTable(
                name: "WasafatyPrescriptions");
        }
    }
}
