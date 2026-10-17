using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartAndPro.ERP.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreDetailsIdToSallaProductMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreDetailsId",
                table: "SallaProductMappings",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SallaProductMappings_StoreDetailsId",
                table: "SallaProductMappings",
                column: "StoreDetailsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SallaProductMappings_StoreDetailsId",
                table: "SallaProductMappings");

            migrationBuilder.DropColumn(
                name: "StoreDetailsId",
                table: "SallaProductMappings");
        }
    }
}
